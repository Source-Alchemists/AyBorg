using System.Collections.Immutable;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Communication;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Plugins.Base.Communication;

public abstract class CommunicationBase : IStepBody
{
    protected SelectPort _devicePort = new("Device", PortDirection.Input, null!);
    protected ImmutableList<IPort> _ports = ImmutableList.Create<IPort>();
    protected IDeviceManager _deviceManager;
    public abstract string Name { get; }
    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.Communication };
    public IReadOnlyCollection<IPort> Ports => _ports;

    protected CommunicationBase(IDeviceManager deviceManager)
    {
        _deviceManager = deviceManager;
        IEnumerable<ICommunicationDevice> devices = _deviceManager.GetDevices<ICommunicationDevice>();
        if (devices.Any())
        {
            _devicePort.Value = new SelectPort.ValueContainer(devices.First().Id, devices.Select(d => d.Id).ToList());
        }

        _ports = _ports.Add(_devicePort);
    }

    public abstract ValueTask<bool> TryRunAsync(CancellationToken cancellationToken);
}
