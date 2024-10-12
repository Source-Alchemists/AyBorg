/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Immutable;
using AyBorg.Types;
using AyBorg.Types.Communication;
using AyBorg.Types.Ports;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Communication;

public abstract class CommunicationBase : IStepBody, IAfterInitialized, IBeforeStart, IDisposable
{
    private readonly ILogger<CommunicationBase> _logger;
    protected readonly SelectPort _devicePort = new("Device", PortDirection.Input, null!);
    protected readonly StringPort _messageIdPort = new("Id", PortDirection.Input, "topic/id");
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
        string selectedId = string.Empty;

        if (devices.Any() && _devicePort.Value != null)
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
