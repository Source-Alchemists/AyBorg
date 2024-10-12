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

using AyBorg.Types;
using AyBorg.Types.ImageAcquisition;
using AyBorg.Types.Ports;

using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.ImageTorque;

public sealed class ImageAcquisition : IStepBody, IAfterInitialized, IBeforeStart, IDisposable
{
    private readonly ILogger<ImageAcquisition> _logger;
    private readonly IDeviceManager _deviceManager;
    private readonly SelectPort _devicePort = new("Device", PortDirection.Input, null!);
    private readonly ImagePort _imagePort = new("Image", PortDirection.Output, null!);
    private readonly NumericPort _indexPort = new("Index", PortDirection.Output, 0);
    private readonly StringPort _infoPort = new("Info", PortDirection.Output, string.Empty);
    private string _lastDeviceId = string.Empty;
    private ICameraDevice? _device;
    private bool _isDisposed = false;

    public IReadOnlyCollection<IPort> Ports { get; }

    public string Name => "Image.Acquisition";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.ImageProcessing };

    public ImageAcquisition(ILogger<ImageAcquisition> logger, IDeviceManager deviceManager)
    {
        _logger = logger;
        _deviceManager = deviceManager;
        Ports = new List<IPort> {
            _devicePort,
            _imagePort,
            _indexPort,
            _infoPort
        };
        _deviceManager.DeviceCollectionChanged += OnDeviceCollectionChanged;
    }

    public ValueTask AfterInitializedAsync()
    {
        IEnumerable<ICameraDevice> devices = _deviceManager.GetDevices<ICameraDevice>();
        UpdateDevicePort(devices);
        ChangeDevice();

        return ValueTask.CompletedTask;
    }

    public ValueTask BeforeStartAsync()
    {
        ChangeDevice();
        return ValueTask.CompletedTask;
    }

    public async ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        try
        {
            ChangeDevice();
            if (_device == null)
            {
                _logger.LogWarning(new EventId((int)EventLogType.Plugin), "No device");
                return false;
            }

            ImageContainer imageContainer = await _device.AcquisitionAsync(cancellationToken);
            _imagePort.Value = imageContainer.Image;
            _indexPort.Value = imageContainer.Index;
            _infoPort.Value = imageContainer.AdditionInfo ?? string.Empty;
            if (imageContainer.Image == null)
            {
                _logger.LogWarning(new EventId((int)EventLogType.Plugin), "No image acquired");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), ex, "Failed acquiring image");
            return false;
        }

        return true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void ChangeDevice()
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

        _device = _deviceManager.GetDevice<ICameraDevice>(_devicePort.Value.SelectedValue);
        if (_device == null && _devicePort.Value != null && !string.IsNullOrEmpty(_devicePort.Value.SelectedValue))
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Device '{deviceId}' not found", _devicePort.Value.SelectedValue);
            return;
        }

        _lastDeviceId = _devicePort.Value!.SelectedValue;
    }

    private void UpdateDevicePort(IEnumerable<ICameraDevice> devices)
    {
        string selectedId = string.Empty;

        if (devices.Any() && _devicePort.Value != null)
        {
            ICameraDevice? selectedDevice = devices.FirstOrDefault(d => d.Id.Equals(_devicePort.Value.SelectedValue, StringComparison.InvariantCultureIgnoreCase));
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
        IEnumerable<ICameraDevice> devices = _deviceManager.GetDevices<ICameraDevice>();
        UpdateDevicePort(devices);
        ChangeDevice();
    }

    private void Dispose(bool isDisposing)
    {
        if (!_isDisposed && isDisposing)
        {
            _deviceManager.DeviceCollectionChanged -= OnDeviceCollectionChanged;
            _isDisposed = true;
        }
    }
}
