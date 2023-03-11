using AyBorg.Diagrams.Core;
using AyBorg.Diagrams.Core.Models;
using AyBorg.Diagrams.Core.Models.Base;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Communication.gRPC.Models;
using AyBorg.Web.Pages.Agent.Editor.Nodes;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Shared.Modals;
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Editor;

#nullable disable

public partial class FlowDiagram : ComponentBase, IDisposable
{
    private readonly Diagram _diagram;
    private bool _suspendDiagramRefresh = false;
    private NotifyService.Subscription _iterationFinishedSubscription = null!;
    private NotifyService.Subscription _flowChangedSubscription = null!;
    private bool _disposedValue;

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
            Subscribe();
        }
    }

    private void Subscribe()
    {
        if (_iterationFinishedSubscription != null) _iterationFinishedSubscription.Callback -= IterationFinishedNotificationReceived;
        _iterationFinishedSubscription = NotifyService.Subscribe(StateService.AgentState.UniqueName, SDK.Communication.gRPC.NotifyType.AgentIterationFinished);
        _iterationFinishedSubscription.Callback += IterationFinishedNotificationReceived;

        if (_flowChangedSubscription != null) _flowChangedSubscription.Callback -= FlowChangedNotificationReceived;
        _flowChangedSubscription = NotifyService.Subscribe(StateService.AgentState.UniqueName, SDK.Communication.gRPC.NotifyType.AgentAutomationFlowChanged);
        _flowChangedSubscription.Callback += FlowChangedNotificationReceived;
    }

    private async void IterationFinishedNotificationReceived(object obj)
    {
        try
        {
            Guid iterationId = (Guid)obj;
            IEnumerable<FlowNode> flowNodes = _diagram.Nodes.Cast<FlowNode>();
            await Parallel.ForEachAsync(flowNodes, async (node, token) =>
            {
                Step newStep = await FlowService.GetStepAsync(node.Step.Id, iterationId);
                if (newStep != null)
                {
                    node.Update(newStep);
                }
                else
                {
                    Logger.LogWarning("Step not found");
                }
            });
        }
        catch (RpcException ex)
        {
            Logger.LogWarning(ex, "Failed to get step");
        }
    }

    private async void FlowChangedNotificationReceived(object obj)
    {
        try
        {
            var flowChangeArgs = (AgentAutomationFlowChangeArgs)obj;

            // Add steps
            foreach (string stepIdStr in flowChangeArgs.AddedSteps)
            {
                Guid stepId = Guid.Parse(stepIdStr);
                if (_diagram.Nodes.Cast<FlowNode>().Any(n => n.Step.Id.Equals(stepId)))
                {
                    // Already exists
                    continue;
                }
                Step newStep = await FlowService.GetStepAsync(stepId);
                await InvokeAsync(() => CreateAndAddNode(newStep));
            }

            // Add links
            foreach (string linkIdStr in flowChangeArgs.AddedLinks)
            {
                Guid linkId = Guid.Parse(linkIdStr);
                if (_diagram.Links.Any(l => l.Id.Equals(linkId.ToString())))
                {
                    // Already exists
                    continue;
                }

                Link newLink = await FlowService.GetLinkAsync(linkId);
                LinkModel linkModel = CreateLinkModel(newLink);
                await InvokeAsync(() => _diagram.Links.Add(linkModel));
                await UpdatePortByLinkModelAsync(linkModel);
            }

            // Remove steps
            foreach (string stepIdStr in flowChangeArgs.RemovedSteps)
            {
                Guid stepId = Guid.Parse(stepIdStr);
                IEnumerable<FlowNode> diagramNodes = _diagram.Nodes.Cast<FlowNode>();

                FlowNode node = diagramNodes.FirstOrDefault(n => n.Step.Id.Equals(stepId));
                if (node == null)
                {
                    // Already removed
                    continue;
                }

                _diagram.Nodes.Removed -= OnNodeRemoved;
                await InvokeAsync(() => _diagram.Nodes.Remove(node));
                _diagram.Nodes.Removed += OnNodeRemoved;
            }

            // Remove links
            foreach (string linkIdStr in flowChangeArgs.RemovedLinks)
            {
                Guid linkId = Guid.Parse(linkIdStr);
                BaseLinkModel link = _diagram.Links.FirstOrDefault(l => l.Id.Equals(linkId.ToString()));
                if (link == null)
                {
                    // Already removed
                    continue;
                }

                await InvokeAsync(() => _diagram.Links.Remove(link));
                await UpdatePortByLinkModelAsync((LinkModel)link);
            }

            // Update steps
            foreach (string stepIdStr in flowChangeArgs.ChangedSteps)
            {
                Guid stepId = Guid.Parse(stepIdStr);
                IEnumerable<FlowNode> diagramNodes = _diagram.Nodes.Cast<FlowNode>();

                FlowNode node = diagramNodes.FirstOrDefault(n => n.Step.Id.Equals(stepId));
                if (node == null)
                {
                    // Already removed
                    continue;
                }

                Step newStep = await FlowService.GetStepAsync(stepId, updatePorts: false);
                await InvokeAsync(() =>
                {
                    node.Update(newStep);
                    node.Refresh();
                });
            }
        }
        catch (RpcException ex)
        {
            Logger.LogWarning(ex, "Failed to get step");
        }
    }

    private async Task CreateFlow()
    {
        try
        {
            IEnumerable<Step> steps = await FlowService.GetStepsAsync();
            // First create all nodes
            foreach (Step step in steps)
            {
                CreateAndAddNode(step);
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

    private void CreateAndAddNode(Step step)
    {
        if (_diagram.Nodes.Cast<FlowNode>().Any(n => n.Step.Id.Equals(step.Id))) return;
        var node = new FlowNode(step, Disabled);
        node.Moving += async (n) => await OnNodeMovingAsync(n);
        node.OnDelete += ShouldDeleteNodeAsync;
        _diagram.Nodes.Add(node);
    }

    private LinkModel CreateLinkModel(Link link)
    {
        NodeModel targetNode = _diagram.Nodes.FirstOrDefault(n => ((FlowNode)n).Step.Ports?.Any(p => p.Id == link.TargetId) != false);
        NodeModel sourceNode = _diagram.Nodes.FirstOrDefault(n => ((FlowNode)n).Step.Ports?.Any(p => p.Id == link.SourceId) != false);
        if (targetNode == null || sourceNode == null)
        {
            Logger.LogWarning("Could not find source or target node");
            return null;
        }
        PortModel targetPort = targetNode.Ports.FirstOrDefault(p => ((FlowPort)p).Port.Id == link.TargetId);
        PortModel sourcePort = sourceNode.Ports.FirstOrDefault(p => ((FlowPort)p).Port.Id == link.SourceId);
        if (targetPort == null || sourcePort == null)
        {
            Logger.LogWarning("Could not find source or target port");
            return null;
        }
        var linkModel = new LinkModel(link.Id.ToString(), sourcePort, targetPort)
        {
            Locked = Disabled
        };
        return linkModel;
    }

    private async ValueTask UpdatePortByLinkModelAsync(LinkModel linkModel)
    {
        if (linkModel.TargetPort != null)
        {
            var tp = (FlowPort)linkModel.TargetPort;
            Port newPort = await FlowService.GetPortAsync(tp.Port.Id);
            tp.Update(newPort);
        }
    }

    private async Task OnLinkTargetPortChangedAsync(BaseLinkModel tmpLink, PortModel p2)
    {
        if (_suspendDiagramRefresh) return;
        if (tmpLink.SourcePort == null) return;

        var fp1 = (FlowPort)tmpLink.SourcePort;
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
            _diagram.Links.Remove(tmpLink);
            return;
        }

        Guid? newLinkId = await FlowService.AddLinkAsync(sourcePort.Port, targetPort.Port);
        Link newLink = await FlowService.GetLinkAsync((Guid)newLinkId);
        if (newLink != null && !tmpLink.Id.Equals(newLink.Id.ToString()))
        {
            _diagram.Links.Remove(tmpLink);
        }
        if (newLink == null)
        {
            Logger.LogWarning("Failed to add link from {Id} to {Id}", sourcePort.Port.Id, targetPort.Port.Id);
            Snackbar.Add("Link is not compatible", Severity.Warning);
        }
        else
        {
            LinkModel linkModel = CreateLinkModel(newLink);
            _diagram.Links.Add(linkModel);
            await UpdatePortByLinkModelAsync(linkModel);
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
        Step step = DragDropStateHandler.DraggedStep;
        if (step == null) return;
        AyBorg.Diagrams.Core.Geometry.Point relativePosition = _diagram.GetRelativeMousePoint(args.ClientX, args.ClientY);
        step.X = (int)relativePosition.X;
        step.Y = (int)relativePosition.Y;

        Step receivedStep = await FlowService.AddStepAsync(step);
        if (receivedStep == null)
        {
            Snackbar.Add($"Could not add '{step.Name}' (Step not found)", Severity.Error);
            return;
        }

        if (!_diagram.Nodes.Cast<FlowNode>().Any(n => n.Step.Id.Equals(receivedStep.Id)))
        {
            CreateAndAddNode(receivedStep);
        }
    }

    private void OnLinkAdded(BaseLinkModel link)
    {
        link.TargetPortChanged += async (l, p1, p2) => await OnLinkTargetPortChangedAsync(l, p2!);
    }

    private async void OnLinkRemovedAsync(BaseLinkModel link)
    {
        if (_suspendDiagramRefresh) return;
        if (link.TargetPort == null || link.SourcePort == null) return; // Nothing to do.
        IEnumerable<Link> links = await FlowService.GetLinksAsync();
        var targetPort = (FlowPort)link.TargetPort;
        Link orgLink = links.FirstOrDefault(l => l.Id.ToString().Equals(link.Id));
        if (orgLink == null) return; // Nothing to do. Already removed.
        if (!await FlowService.TryRemoveLinkAsync(orgLink))
        {
            _diagram.Links.Add(link);
            return;
        }

        Port newPort = await FlowService.GetPortAsync(targetPort.Port.Id);
        targetPort.Update(newPort);
    }

    private async void OnNodeRemoved(NodeModel node)
    {
        if (_suspendDiagramRefresh) return;
        if (node is FlowNode flowNode
            && !await FlowService.TryRemoveStepAsync(flowNode.Step))
        {
            _diagram.Nodes.Add(node);
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
        foreach (NodeModel node in selectedNodes)
        {
            if (await ShowDeleteConfirmDialogAsync(node.Title))
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

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (_iterationFinishedSubscription != null) _iterationFinishedSubscription.Callback -= IterationFinishedNotificationReceived;
                if (_flowChangedSubscription != null) _flowChangedSubscription.Callback -= FlowChangedNotificationReceived;
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
