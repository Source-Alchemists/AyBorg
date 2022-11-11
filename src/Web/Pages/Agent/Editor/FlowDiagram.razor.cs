using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using MudBlazor;
using Autodroid.SDK.Data.DTOs;
using Autodroid.Web.Pages.Agent.Editor.Nodes;
using Autodroid.Web.Services.Agent;
using Autodroid.SDK.Communication.MQTT;
using Autodroid.Web.Services;
using Autodroid.Web.Shared.Modals;

namespace Autodroid.Web.Pages.Agent.Editor;

public partial class FlowDiagram : ComponentBase, IAsyncDisposable
{
    private readonly Diagram _diagram;
    private HubConnection _flowHubConnection = null!;
    private bool _suspendDiagramRefresh = false;

    [Inject] IFlowService FlowService { get; set; } = null!;
    [Inject] IMqttClientProvider MqttClientProvider { get; set; } = null!;
    [Inject] ILogger<FlowDiagram> Logger { get; set; } = null!;
    [Inject] IStateService StateService { get; set; } = null!;
    [Inject] ISnackbar Snackbar { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;

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
        _diagram.Nodes.Removed += async (n) => await OnNodeRemovedAsync(n);
        _diagram.Links.Added += (l) => OnLinkAdded(l);
        _diagram.Links.Removed += (l) => OnLinkRemovedAsync(l);
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

            await ConnectHubEventsAsync();
            await CreateFlow();
            _diagram.Refresh();
            var zoom = await StateService.UpdateAutomationFlowZoomFromLocalstorageAsync();
            _diagram.SetZoom(zoom);
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

    private async Task ConnectHubEventsAsync()
    {
        _flowHubConnection = FlowService.CreateHubConnection(StateService.AgentState.BaseUrl);
        ConnectLinkChangedHubEvent();
        await _flowHubConnection.StartAsync();
    }

    private void ConnectLinkChangedHubEvent()
    {
        _flowHubConnection.On<LinkDto, bool>("LinkChanged", async (linkDto, removed) =>
        {
            Logger.LogTrace("LinkChanged event received from server. Link: {link}, Removed: {removed}", linkDto, removed);
            if (removed)
            {
                var linkModel = FindLinkModel(linkDto);
                if (linkModel != null)
                {
                    _diagram.Links.Remove(linkModel);
                }

                var tn = _diagram.Nodes.Cast<FlowNode>().FirstOrDefault(n => n.Step.Ports?.Any(p => p.Id == linkDto.TargetId) != false);
                if (tn == null) return;
                var tp = tn.Ports.Cast<FlowPort>().First(p => p.Port.Id.Equals(linkDto.TargetId));
                await tp.UpdateAsync();
            }
            else
            {
                var link = FindLinkModel(linkDto);

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

    private BaseLinkModel FindLinkModel(LinkDto linkDto)
    {
        foreach (var li in _diagram.Links)
        {
            if (li.SourcePort is FlowPort sp && sp.Port.Id.Equals(linkDto.SourceId) && li.TargetPort is FlowPort tp && tp.Port.Id.Equals(linkDto.TargetId))
            {
                return li;
            }
        }

        return null!;
    }

    private FlowNode CreateNode(StepDto step)
    {
        var node = new FlowNode(FlowService, MqttClientProvider, StateService, step);
        node.Moving += async (n) => await OnNodeMovingAsync(n);
        node.OnDelete += ShouldDeleteNodeAsync;
        return node;
    }

    private async Task CreateFlow()
    {
        try
        {
            var steps = await FlowService.GetStepsAsync(StateService.AgentState.BaseUrl);
            // First create all nodes
            foreach (var step in steps)
            {
                _diagram.Nodes.Add(CreateNode(step));
            }
            // Next create all links
            var links = await FlowService.GetLinksAsync(StateService.AgentState.BaseUrl);
            var linkHashes = new HashSet<LinkDto>();
            foreach (var link in links)
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

    private LinkModel CreateLinkModel(LinkDto link)
    {
        var targetNode = _diagram.Nodes.FirstOrDefault(n => ((FlowNode)n).Step.Ports?.Any(p => p.Id == link.TargetId) != false);
        var sourceNode = _diagram.Nodes.FirstOrDefault(n => ((FlowNode)n).Step.Ports?.Any(p => p.Id == link.SourceId) != false);
        if (targetNode == null || sourceNode == null)
        {
            throw new Exception("Could not find source or target node");
        }
        var targetPort = targetNode.Ports.FirstOrDefault(p => ((FlowPort)p).Port.Id == link.TargetId);
        var sourcePort = sourceNode.Ports.FirstOrDefault(p => ((FlowPort)p).Port.Id == link.SourceId);
        if (targetPort == null || sourcePort == null)
        {
            throw new Exception("Could not find source or target port");
        }
        var linkModel = new LinkModel(link.Id.ToString(), sourcePort, targetPort);
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

        var result = await FlowService.TryAddLinkAsync(StateService.AgentState.BaseUrl, sourcePort.Port.Id, targetPort.Port.Id);
        if (!result)
        {
            Logger.LogWarning("Failed to add link from {Id} to {Id}", sourcePort.Port.Id, targetPort.Port.Id);
            _diagram.Links.Remove(link);
            Snackbar.Add("Link is not compatible", Severity.Warning);
        }
    }

    private static async ValueTask OnDragEnter(DragEventArgs args)
    {
        if (DragDropStateHandler.DraggedStep == null) args.DataTransfer.DropEffect = "none";
        else args.DataTransfer.DropEffect = "move";
        await ValueTask.CompletedTask;
    }

    private async Task OnDrop(DragEventArgs args)
    {
        if (_diagram == null) return;
        var step = DragDropStateHandler.DraggedStep;
        if (step == null) return;
        var relativePosition = _diagram.GetRelativeMousePoint(args.ClientX, args.ClientY);
        step.X = (int)relativePosition.X;
        step.Y = (int)relativePosition.Y;

        var receivedStep = await FlowService.AddStepAsync(StateService.AgentState.BaseUrl, step.Id, step.X, step.Y);
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
        var links = await FlowService.GetLinksAsync(StateService.AgentState.BaseUrl);
        var sp = (FlowPort)link.SourcePort;
        var tp = (FlowPort)link.TargetPort;
        var orgLink = links.FirstOrDefault(l => l.SourceId == sp.Port.Id && l.TargetId == tp.Port.Id);
        if (orgLink == null) return; // Nothing to do. Already removed.
        if (!await FlowService.TryRemoveLinkAsync(StateService.AgentState.BaseUrl, orgLink.Id))
        {
            _diagram.Links.Add(link);
            return;
        }

        await tp.UpdateAsync();
    }

    private async Task OnNodeRemovedAsync(NodeModel node)
    {
        if (_suspendDiagramRefresh) return;
        if (node is FlowNode flowNode)
        {
            if (await FlowService.TryRemoveStepAsync(StateService.AgentState.BaseUrl, flowNode.Step.Id))
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
            await FlowService.TryMoveStepAsync(StateService.AgentState.BaseUrl, flowNode.Step.Id, (int)flowNode.Position.X, (int)flowNode.Position.Y);
        }
    }

    private async void ShouldDeleteNodeAsync()
    {
        var selectedNodes = _diagram.Nodes.Where(n => n.Selected).ToList();
        foreach (var node in selectedNodes)
        {
            await ShowDeleteConfirmDialogAsync(node.Title);
            await OnNodeRemovedAsync(node);
        }
    }

    private async Task<bool> ShouldDeleteNodeAsync(NodeModel model) => await ShowDeleteConfirmDialogAsync(model.Title);

    private async Task<bool> ShowDeleteConfirmDialogAsync(string targetName)
    {
        var dialogParameters = new DialogParameters
        {
            { "ContentText", $"Are you sure you want to delete '{targetName}'?" }
        };
        var dialog = DialogService.Show<ConfirmDialog>("Confirm deletion", dialogParameters, new DialogOptions());
        var result = await dialog.Result;
        return !result.Cancelled;
    }

    private async void OnZoomInClicked()
    {
        _diagram.SetZoom(_diagram.Zoom + 0.1);
        await StateService.SetAutomationFlowZoomAsync(_diagram.Zoom);
    }
    private async void OnZoomOutClicked()
    {
        _diagram.SetZoom(_diagram.Zoom - 0.1);
        await StateService.SetAutomationFlowZoomAsync(_diagram.Zoom);

    }
    private async void OnZoomResetClicked()
    {
        _diagram.SetZoom(1.0);
        await StateService.SetAutomationFlowZoomAsync(_diagram.Zoom);
    }
}