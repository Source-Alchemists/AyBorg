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

using AyBorg.Runtime.Devices;
using AyBorg.Types;
using AyBorg.Types.Models;
using AyBorg.Types.Ports;
using SR = System.Reflection;

namespace AyBorg.Agent;

public sealed class DeviceProxy : IDeviceProxy
{
    private bool _isDisposed = false;
    public string Id => Native.Id;
    public string Name => Native.Name;
    public string Manufacturer => Native.Manufacturer;
    public bool IsActive { get; private set; }
    public bool IsConnected => Native.IsConnected;
    public IReadOnlyCollection<string> Categories => Native.Categories;

    /// <summary>
    /// Gets the ports.
    /// </summary>
    public IEnumerable<IPort> Ports => Native.Ports;

    /// <summary>
    /// Gets or sets the meta information.
    /// </summary>
    public PluginMetaInfo MetaInfo { get; private set; } = new PluginMetaInfo();

    /// <summary>
    /// Gets or sets the provider meta information.
    /// </summary>
    public PluginMetaInfo ProviderMetaInfo { get; private set; } = new PluginMetaInfo();

    public IDevice Native { get; }

    public DeviceProxy(ILogger<IDeviceProxy> logger, IDeviceProvider parent, IDevice device, bool isActive)
    {
        Native = device;
        IsActive = isActive;
        FillDeviceMetaInfo(device);
        FillDeviceProviderMetaInfo(parent);
    }

    private void FillDeviceMetaInfo(IDevice device)
    {
        string typeName = device.GetType().Name;
        SR.Assembly assembly = device.GetType().Assembly;
        SR.AssemblyName? assemblyName = assembly?.GetName();

        MetaInfo = new PluginMetaInfo
        {
            TypeName = typeName,
            AssemblyName = assemblyName!.Name!,
            AssemblyVersion = assemblyName!.Version!.ToString()
        };
    }

    private void FillDeviceProviderMetaInfo(IDeviceProvider deviceProvider)
    {
        string typeName = deviceProvider.GetType().Name;
        SR.Assembly assembly = deviceProvider.GetType().Assembly;
        SR.AssemblyName? assemblyName = assembly?.GetName();

        ProviderMetaInfo = new PluginMetaInfo
        {
            TypeName = typeName,
            AssemblyName = assemblyName!.Name!,
            AssemblyVersion = assemblyName!.Version!.ToString()
        };
    }

    public async ValueTask<bool> TryConnectAsync()
    {
        IsActive = await Native.TryConnectAsync();
        return IsActive;
    }

    public async ValueTask<bool> TryDisconnectAsync()
    {
        IsActive = !await Native.TryDisconnectAsync();
        return !IsActive;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
        if (isDisposing && !_isDisposed)
        {
            if (Native is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _isDisposed = true;
        }
    }
}
