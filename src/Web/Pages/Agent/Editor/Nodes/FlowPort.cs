using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.Diagrams.Core.Models;

namespace AyBorg.Web.Pages.Agent.Editor.Nodes;

public class FlowPort : PortModel
{
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
    /// Initializes a new instance of the <see cref="FlowPort"/> class.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="port">The port.</param>
    public FlowPort(FlowNode node, Port port)
            : base(port.Id.ToString(), node, port.Direction == PortDirection.Input ? PortAlignment.Left : PortAlignment.Right)
    {
        Port = port;
        Name = port.Name;
        Direction = port.Direction;
        Brand = port.Brand;
    }

    /// <summary>
    /// Updates the port.
    /// </summary>
    public void Update(Port newPort)
    {
        if (newPort.Id == Guid.Empty) return;
        Port = newPort;
    }
}
