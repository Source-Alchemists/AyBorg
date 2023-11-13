using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Widgets;

public class GaugeWidget : BaseWidget
{
    private readonly StringPort _keyPort = new("Key", PortDirection.Input, string.Empty);
    private readonly EnumPort _unitPort = new("Unit", PortDirection.Input, Unit.Milliseconds);
    private readonly NumericCollectionPort _numericCollectionPort = new("Values", PortDirection.Output);
    public override string DefaultName => "Gauge";

    public GaugeWidget() : base()
    {
        _ports = _ports.Add(_keyPort);
        _ports = _ports.Add(_unitPort);
        _ports = _ports.Add(_numericCollectionPort);
    }

}
