using System.Collections.Immutable;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Communication;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Plugins.Base.Communication;

public abstract class CommunicationBase : IStepBody, IAfterInitialized
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
        _ports = _ports.Add(_devicePort);
    }

    public ValueTask AfterInitializedAsync()
    {
        IEnumerable<ICommunicationDevice> devices = _deviceManager.GetDevices<ICommunicationDevice>();
        if (devices.Any())
        {
            string selectedId = string.Empty;

            if (_devicePort.Value != null)
            {
                ICommunicationDevice? selectedDevice = devices.FirstOrDefault(d => d.Id.Equals(_devicePort.Value.SelectedValue, StringComparison.InvariantCultureIgnoreCase));
                if (selectedDevice != null)
                {
                    selectedId = selectedDevice.Id;
                }
                else
                {
                    selectedId = devices.First().Id;
                }
            }

            _devicePort.Value = new SelectPort.ValueContainer(selectedId, devices.Select(d => d.Id).ToList());
        }

        return ValueTask.CompletedTask;
    }

    public abstract ValueTask<bool> TryRunAsync(CancellationToken cancellationToken);

}
