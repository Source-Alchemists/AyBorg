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
using AyBorg.Runtime.Devices;
using AyBorg.Types;
using AyBorg.Types.Models;
using SR = System.Reflection;

namespace AyBorg.Agent;

public sealed class DeviceProviderProxy : IDeviceProviderProxy
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<DeviceProviderProxy> _logger;
    private readonly IDeviceProvider _deviceProvider;
    private ImmutableList<IDeviceProxy> _devices = ImmutableList.Create<IDeviceProxy>();
    private bool _isDisposed = false;

    /// <inheritdoc/>
    public string Name => _deviceProvider.Name;

    /// <inheritdoc/>
    public string Prefix => _deviceProvider.Prefix;

    /// <inheritdoc/>
    public bool CanAdd => _deviceProvider.CanCreate;

    /// <inheritdoc/>
    public PluginMetaInfo MetaInfo { get; }

    /// <inheritdoc/>
    public IReadOnlyCollection<IDeviceProxy> Devices => _devices;

    /// <inheritdoc/>
    public DeviceProviderProxy(ILoggerFactory loggerFactory, ILogger<DeviceProviderProxy> logger, IDeviceProvider deviceProvider)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
        _deviceProvider = deviceProvider;

        string typeName = deviceProvider.GetType().Name;
        SR.Assembly assembly = deviceProvider.GetType().Assembly;
        SR.AssemblyName? assemblyName = assembly?.GetName();

        MetaInfo = new PluginMetaInfo
        {
            TypeName = typeName,
            AssemblyName = assemblyName!.Name!,
            AssemblyVersion = assemblyName!.Version!.ToString()
        };
    }

    /// <inheritdoc/>
    public async ValueTask<bool> TryInitializeAsync()
    {
        try
        {
            if (_deviceProvider is IAfterInitialized initializable)
            {
                await initializable.AfterInitializedAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning((int)EventLogType.Plugin, ex, "Failed to initialize device manager '{name}'", _deviceProvider.Name);
            return false;
        }
    }

    /// <inheritdoc/>
    public async ValueTask<IDeviceProxy> AddAsync(AddDeviceOptions options)
    {
        if (!_deviceProvider.CanCreate)
        {
            throw new InvalidOperationException("Device manager does not support adding devices");
        }

        if (Devices.Any(d => d.Id.Equals(options.DeviceId, StringComparison.InvariantCultureIgnoreCase)))
        {
            throw new InvalidOperationException($"Device with name '{options.DeviceId}' already exists");
        }

        IDevice device = await _deviceProvider.CreateAsync(options.DeviceId);
        var deviceProxy = new DeviceProxy(_loggerFactory.CreateLogger<IDeviceProxy>(), _deviceProvider, device, options.IsActive);
        _devices = _devices.Add(deviceProxy);
        _logger.LogInformation((int)EventLogType.Plugin, "Added device '{id}'", options.DeviceId);
        return deviceProxy;
    }

    /// <inheritdoc/>
    public async ValueTask<IDeviceProxy> RemoveAsync(string id)
    {

        IDeviceProxy? deviceProxy = Devices.FirstOrDefault(d => d.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase)) ?? throw new KeyNotFoundException($"Device with id '{id}' does not exist");

        if (deviceProxy.IsConnected && !await deviceProxy.TryDisconnectAsync())
        {
            throw new InvalidOperationException($"Failed to disconnect device '{id}'");
        }

        deviceProxy.Dispose();

        _devices = _devices.Remove(deviceProxy);
        _logger.LogInformation((int)EventLogType.Plugin, "Removed device '{id}'", id);
        return deviceProxy;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            foreach (IDeviceProxy deviceProxy in _devices)
            {
                deviceProxy.Dispose();
            }

            if (_deviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _isDisposed = true;
        }
    }
}
