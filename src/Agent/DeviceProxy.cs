using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent;

public class DeviceProxy : IDeviceProxy
{
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
        System.Reflection.Assembly assembly = device.GetType().Assembly;
        System.Reflection.AssemblyName? assemblyName = assembly?.GetName();

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
        System.Reflection.Assembly assembly = deviceProvider.GetType().Assembly;
        System.Reflection.AssemblyName? assemblyName = assembly?.GetName();

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
}
