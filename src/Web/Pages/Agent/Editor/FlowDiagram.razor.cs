using AyBorg.SDK.Common.Models;
using AyBorg.Web.Pages.Agent.Editor.Nodes;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Services.AppState;
using AyBorg.Web.Shared.Modals;
using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using AyBorg.Web.Services;

namespace AyBorg.Web.Pages.Agent.Editor;

public partial class FlowDiagram : ComponentBase, IAsyncDisposable
{
    private readonly Diagram _diagram;
    private HubConnection _flowHubConnection = null!;
    private bool _suspendDiagramRefresh = false;

    [Inject] IFlowService FlowService { get; set; } = null!;
    [Inject] INotifyService NotifyService { get; set; } = null!;
    [Inject] ILogger<FlowDiagram> Logger { get; set; } = null!;
    [Inject] IStateService StateService { get; set; } = null!;
    [Inject] ISnackbar Snackbar { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;

    [Parameter] public bool Disabled { get; set; } = false;

    public FlowDiagram()
    {
        var options = new DiagramOptions
        {
            DeleteKey = "Delete",
            AllowMultiSelection = true,
            GridSize = 40,
            EnableVirtualization = false, // Workaround for a bug in Blazor.Diagrams. See: https://github.com/Blazor-Diagrams/Blazor.Diagrams/issues/155
            Zoom = new DiagramZoomOptions
            {
                Minimum = 0.25,
                Maximum = 3,
                Inverse = true
            },
            DefaultNodeComponent = typeof(FlowNodeWidget),
            Links = new DiagramLinkOptions
            {
                DefaultColor = "#8391A2",
                DefaultSelectedColor = "#6366F1"
            },
            Constraints = new DiagramConstraintsOptions
            {
                ShouldDeleteNode = ShouldDeleteNodeAsync
            }
        };

        _diagram = new Diagram(options);
        _diagram.RegisterModelComponent<FlowNode, FlowNodeWidget>();
        _diagram.Nodes.Removed += OnNodeRemoved;
        _diagram.Links.Added += OnLinkAdded;
        _diagram.Links.Removed += OnLinkRemovedAsync;
        _diagram.ZoomChanged += OnZoomChanged;
        _diagram.PanChanged += OnPanChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            _suspendDiagramRefresh = true;
            _diagram.Nodes.Clear();
            _diagram.Links.Clear();
            _suspendDiagramRefresh = false;

            // await ConnectHubEventsAsync();
            double zoom = await StateService.AutomationFlowState.UpdateZoomAsync();
            _diagram.SetZoom(zoom);
            (double offsetX, double offsetY) = await StateService.AutomationFlowState.UpdateOffsetAsync();
            _diagram.SetPan(offsetX, offsetY);
            await CreateFlow();
            if (Disabled)
            {
                _diagram.Locked = true;
                _diagram.Nodes.Removed -= OnNodeRemoved;
                _diagram.Links.Added -= OnLinkAdded;
                _diagram.Links.Removed -= OnLinkRemovedAsync;
                _diagram.ZoomChanged -= OnZoomChanged;
                _diagram.PanChanged -= OnPanChanged;
            }
            _diagram.Refresh();
        }
    }

    /// <summary>
    /// Updates the component.
    /// </summary>
    public async Task UpdateAsync()
    {
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Dispose the component.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_flowHubConnection != null) await _flowHubConnection.DisposeAsync();
    }

    // private async Task ConnectHubEventsAsync()
    // {
    //     _flowHubConnection = FlowService.CreateHubConnection(StateService.AgentState.BaseUrl);
    //     ConnectLinkChangedHubEvent();
    //     await _flowHubConnection.StartAsync();
    // }

    private void ConnectLinkChangedHubEvent()
    {
        _flowHubConnection.On<Link, bool>("LinkChanged", async (linkDto, removed) =>
        {
            Logger.LogTrace("LinkChanged event received from server. Link: {link}, Removed: {removed}", linkDto, removed);
            if (removed)
            {
                BaseLinkModel linkModel = FindLinkModel(linkDto);
                if (linkModel != null)
                {
                    _diagram.Links.Remove(linkModel);
                }

                FlowNode? tn = _diagram.Nodes.Cast<FlowNode>().FirstOrDefault(n => n.Step.Ports?.Any(p => p.Id == linkDto.TargetId) != false);
                if (tn == null) return;
                FlowPort tp = tn.Ports.Cast<FlowPort>().First(p => p.Port.Id.Equals(linkDto.TargetId));
                await tp.UpdateAsync();
            }
            else
            {
                BaseLinkModel link = FindLinkModel(linkDto);

                if (link != null)
                {
                    if (link.TargetPort != null)
                    {
                        var targetPort = (FlowPort)link.TargetPort;
                        await targetPort.UpdateAsync();
                    }
                    return;
                }

                _diagram.Links.Add(CreateLinkModel(linkDto));
            }
        });
    }

    private BaseLinkModel FindLinkModel(Link linkDto)
    {
        foreach (BaseLinkModel li in _diagram.Links)
        {
            if (li.SourcePort is FlowPort sp && sp.Port.Id.Equals(linkDto.SourceId) && li.TargetPort is FlowPort tp && tp.Port.Id.Equals(linkDto.TargetId))
            {
                return li;
            }
        }

        return null!;
    }

    private FlowNode CreateNode(Step step)
    {
        var node = new FlowNode(FlowService, NotifyService, StateService, step, Disabled);
        node.Moving += async (n) => await OnNodeMovingAsync(n);
        node.OnDelete += ShouldDeleteNodeAsync;
        return node;
    }

    private async Task CreateFlow()
    {
        try
        {
            IEnumerable<Step> steps = await FlowService.GetStepsAsync();
            // First create all nodes
            foreach (Step step in steps)
            {
                _diagram.Nodes.Add(CreateNode(step));
            }
            // Next create all links
            IEnumerable<Link> links = await FlowService.GetLinksAsync();
            var linkHashes = new HashSet<Link>();
            foreach (Link link in links)
            {
                if (linkHashes.Contains(link))
                {
                    continue;
                }

                linkHashes.Add(link);
                _diagram.Links.Add(CreateLinkModel(link));
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error while creating flow");
        }
    }

    private LinkModel CreateLinkModel(Link link)
    {
        NodeModel? targetNode = _diagram.Nodes.FirstOrDefault(n => ((FlowNode)n).Step.Ports?.Any(p => p.Id == link.TargetId) != false);
        NodeModel? sourceNode = _diagram.Nodes.FirstOrDefault(n => ((FlowNode)n).Step.Ports?.Any(p => p.Id == link.SourceId) != false);
        if (targetNode == null || sourceNode == null)
        {
            throw new Exception("Could not find source or target node");
        }
        PortModel? targetPort = targetNode.Ports.FirstOrDefault(p => ((FlowPort)p).Port.Id == link.TargetId);
        PortModel? sourcePort = sourceNode.Ports.FirstOrDefault(p => ((FlowPort)p).Port.Id == link.SourceId);
        if (targetPort == null || sourcePort == null)
        {
            throw new Exception("Could not find source or target port");
        }
        var linkModel = new LinkModel(link.Id.ToString(), sourcePort, targetPort)
        {
            Locked = Disabled
        };
        return linkModel;
    }


    private async Task OnLinkTargetPortChangedAsync(BaseLinkModel link, PortModel _, PortModel p2)
    {
        if (_suspendDiagramRefresh) return;
        if (link.SourcePort == null) return;

        var fp1 = (FlowPort)link.SourcePort;
        var fp2 = (FlowPort)p2;
        FlowPort sourcePort;
        FlowPort targetPort;

        // Just basic validation, the rest is done in the backend
        if (fp1.Port.Direction == SDK.Common.Ports.PortDirection.Output)
        {
            sourcePort = fp1;
            targetPort = fp2;
        }
        else
        {
            sourcePort = fp2;
            targetPort = fp1;
        }

        if (targetPort.Port.IsConnected)
        {
            Logger.LogWarning("Port {targetPort.Port.Id} already has a link", targetPort.Port.Id);
            Snackbar.Add("Port already has a link", Severity.Warning);
            _diagram.Links.Remove(link);
            return;
        }

        bool result = await FlowService.TryAddLinkAsync(sourcePort.Port.Id, targetPort.Port.Id);
        if (!result)
        {
            Logger.LogWarning("Failed to add link from {Id} to {Id}", sourcePort.Port.Id, targetPort.Port.Id);
            _diagram.Links.Remove(link);
            Snackbar.Add("Link is not compatible", Severity.Warning);
        }
    }

    private static async Task OnDragEnter(DragEventArgs args)
    {
        if (DragDropStateHandler.DraggedStep == null) args.DataTransfer.DropEffect = "none";
        else args.DataTransfer.DropEffect = "move";
        await ValueTask.CompletedTask;
    }

    private async Task OnDrop(DragEventArgs args)
    {
        if (_diagram == null) return;
        Step? step = DragDropStateHandler.DraggedStep;
        if (step == null) return;
        Blazor.Diagrams.Core.Geometry.Point relativePosition = _diagram.GetRelativeMousePoint(args.ClientX, args.ClientY);
        step.X = (int)relativePosition.X;
        step.Y = (int)relativePosition.Y;

        Step receivedStep = await FlowService.AddStepAsync(step.Id, step.X, step.Y);
        if (receivedStep != null)
        {
            _diagram.Nodes.Add(CreateNode(receivedStep));
        }
    }

    private void OnLinkAdded(BaseLinkModel link)
    {
        link.TargetPortChanged += async (l, p1, p2) => await OnLinkTargetPortChangedAsync(l, p1!, p2!);
    }

    private async void OnLinkRemovedAsync(BaseLinkModel link)
    {
        if (_suspendDiagramRefresh) return;
        if (link.TargetPort == null || link.SourcePort == null) return; // Nothing to do.
        IEnumerable<Link> links = await FlowService.GetLinksAsync();
        var sp = (FlowPort)link.SourcePort;
        var tp = (FlowPort)link.TargetPort;
        Link? orgLink = links.FirstOrDefault(l => l.SourceId == sp.Port.Id && l.TargetId == tp.Port.Id);
        if (orgLink == null) return; // Nothing to do. Already removed.
        if (!await FlowService.TryRemoveLinkAsync(orgLink.Id))
        {
            _diagram.Links.Add(link);
            return;
        }

        await tp.UpdateAsync();
    }

    private async void OnNodeRemoved(NodeModel node)
    {
        if (_suspendDiagramRefresh) return;
        if (node is FlowNode flowNode)
        {
            if (await FlowService.TryRemoveStepAsync(flowNode.Step.Id))
            {
                _diagram.Nodes.Remove(node);
            }
        }
    }

    private async Task OnNodeMovingAsync(NodeModel node)
    {
        if (_suspendDiagramRefresh) return;
        if (node is FlowNode flowNode)
        {
            await FlowService.TryMoveStepAsync(flowNode.Step.Id, (int)flowNode.Position.X, (int)flowNode.Position.Y);
        }
    }

    private async void ShouldDeleteNodeAsync()
    {
        var selectedNodes = _diagram.Nodes.Where(n => n.Selected).ToList();
        foreach (NodeModel? node in selectedNodes)
        {
            if(await ShowDeleteConfirmDialogAsync(node.Title))
            {
                OnNodeRemoved(node);
            }
        }
    }

    private async Task<bool> ShouldDeleteNodeAsync(NodeModel model) => await ShowDeleteConfirmDialogAsync(model.Title);

    private async Task<bool> ShowDeleteConfirmDialogAsync(string targetName)
    {
        var dialogParameters = new DialogParameters
        {
            { "ContentText", $"Are you sure you want to delete '{targetName}'?" }
        };
        IDialogReference dialog = DialogService.Show<ConfirmDialog>("Confirm deletion", dialogParameters, new DialogOptions());
        DialogResult result = await dialog.Result;
        return !result.Cancelled;
    }

    private void OnZoomInClicked() => _diagram.SetZoom(_diagram.Zoom + 0.1);
    private void OnZoomOutClicked() => _diagram.SetZoom(_diagram.Zoom - 0.1);
    private void OnZoomResetClicked() => _diagram.SetZoom(1.0);

    private async void OnZoomChanged()
    {
        await StateService.AutomationFlowState.SetZoomAsync(_diagram.Zoom);
    }

    private async void OnPanChanged()
    {
        await StateService.AutomationFlowState.SetOffsetAsync(_diagram.Pan.X, _diagram.Pan.Y);
    }
}
