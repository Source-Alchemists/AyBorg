using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Widgets;

public abstract class BaseWidget
{
    public abstract string DefaultName { get; }

    [Required]
    public string TypeName { get; init; } = string.Empty;

    public Point Position { get; init; }

    public IEnumerable<IPort> Ports => _ports;

    protected ImmutableList<IPort> _ports = ImmutableList.Create<IPort>();

    protected BaseWidget()
    {
        _ports = _ports.Add(new StringPort("Name", PortDirection.Input, DefaultName));
    }
}
