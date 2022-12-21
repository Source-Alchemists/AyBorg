using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.Web.Services.Agent;
using Blazor.Diagrams.Core.Models;

namespace AyBorg.Web.Pages.Agent.Editor.Nodes;

public class FlowPort : PortModel
{
    private readonly IFlowService _flowService;
    private readonly Step _step;

    /// <summary>
    /// Gets the port name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the port direction.
    /// </summary>
    public PortDirection Direction { get; }

    /// <summary>
    /// Gets the port type.
    /// </summary>
    public PortBrand Brand { get; }

    /// <summary>
    /// Gets the port dto.
    /// </summary>
    public Port Port { get; private set; }

    /// <summary>
    /// Called when a link is added or removed.
    /// </summary>
    public Action? PortChanged { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowPort"/> class.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="port">The port.</param>
    /// <param name="parent">The parent.</param>
    /// <param name="flowService">The flow service.</param>
    /// <param name="stateService">The state service.</param>
    public FlowPort(FlowNode node, Port port, Step parent, IFlowService flowService)
            : base(node, port.Direction == PortDirection.Input ? PortAlignment.Left : PortAlignment.Right)
    {
        Port = port;
        Name = port.Name;
        Direction = port.Direction;
        Brand = port.Brand;
        _flowService = flowService;
        _step = parent;
    }

    /// <summary>
    /// Updates the port.
    /// </summary>
    public async Task UpdateAsync()
    {
        Port newPort = await _flowService.GetPortAsync(Port.Id);
        if (newPort == null) return;
        Port = newPort;
        PortChanged?.Invoke();
    }

    /// <summary>
    /// Sends the value to the server.
    /// </summary>
    public async Task SendValueAsync()
    {
        if (!await _flowService.TrySetPortValueAsync(Port))
        {
            throw new Exception("Failed to set port value.");
        }
    }

    private async void NotificationCallback(object obj)
    {
        if(!Guid.TryParse(obj.ToString(), out Guid iterationId)) return;
        Port updatedPort = await _flowService.GetPortAsync(Port.Id, iterationId);
        Port.Value = updatedPort.Value;
        PortChanged?.Invoke();
    }
}
