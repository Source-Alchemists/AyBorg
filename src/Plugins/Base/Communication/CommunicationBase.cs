using System.Collections.Immutable;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Communication;
using AyBorg.SDK.Common.Ports;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Communication;

public abstract class CommunicationBase : IStepBody, IAfterInitialized, IBeforeStart, IDisposable
{
    private readonly ILogger<CommunicationBase> _logger;
    protected readonly SelectPort _devicePort = new("Device", PortDirection.Input, null!);
    protected readonly StringPort _messageIdPort = new("Id", PortDirection.Input, string.Empty);
    private string _lastDeviceId = string.Empty;
    private bool _isDisposed = false;
    protected ImmutableList<IPort> _ports = ImmutableList.Create<IPort>();
    protected IDeviceManager _deviceManager;
    protected ICommunicationDevice? _device;
    public abstract string Name { get; }
    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.Communication };
    public IReadOnlyCollection<IPort> Ports => _ports;

    protected CommunicationBase(ILogger<CommunicationBase> logger, IDeviceManager deviceManager)
    {
        _logger = logger;
        _deviceManager = deviceManager;
        _ports = _ports.Add(_devicePort);
        _deviceManager.DeviceCollectionChanged += OnDeviceCollectionChanged;
    }

    public ValueTask AfterInitializedAsync()
    {
        IEnumerable<ICommunicationDevice> devices = _deviceManager.GetDevices<ICommunicationDevice>();
        UpdateDevicePort(devices);
        ChangeDevice();

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask BeforeStartAsync()
    {
        ChangeDevice();
        return ValueTask.CompletedTask;
    }

    protected void ChangeDevice()
    {
        if (_devicePort.Value == null)
        {
            _lastDeviceId = string.Empty;
            return;
        }

        if (_lastDeviceId.Equals(_devicePort.Value.SelectedValue))
        {
            return;
        }

        _device = _deviceManager.GetDevice<ICommunicationDevice>(_devicePort.Value.SelectedValue);
        if (_device == null && _devicePort.Value != null && !string.IsNullOrEmpty(_devicePort.Value.SelectedValue))
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Device '{deviceId}' not found", _devicePort.Value.SelectedValue);
            return;
        }

        _lastDeviceId = _devicePort.Value!.SelectedValue;
    }

    private void UpdateDevicePort(IEnumerable<ICommunicationDevice> devices)
    {
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
    }

    private void OnDeviceCollectionChanged(object? sender, CollectionChangedEventArgs e)
    {
        IEnumerable<ICommunicationDevice> devices = _deviceManager.GetDevices<ICommunicationDevice>();
        UpdateDevicePort(devices);
        ChangeDevice();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool isDisposing)
    {
        if (!_isDisposed && isDisposing)
        {
            _deviceManager.DeviceCollectionChanged -= OnDeviceCollectionChanged;
            _isDisposed = true;
        }
    }

    public abstract ValueTask<bool> TryRunAsync(CancellationToken cancellationToken);
}
