using AyBorg.SDK.Common;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.ImageTorque;

public sealed class VirtualDeviceProvider : IDeviceProvider
{
    private readonly ILogger<VirtualDeviceProvider> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IEnvironment _environment;

    public string Prefix => "AyBV";

    public bool CanCreate => true;

    public string Name => "Virtual Devices";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultDeviceCategories.Camera, "Virtual Device" };

    public VirtualDeviceProvider(ILogger<VirtualDeviceProvider> logger, ILoggerFactory loggerFactory, IEnvironment environment)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _environment = environment;
    }

    public async ValueTask<IDevice> CreateAsync(string id)
    {
        var device = new VirtualDevice(_loggerFactory.CreateLogger<VirtualDevice>(), _environment, id);
        _logger.LogTrace((int)EventLogType.Plugin, "Added virtual device '{id}'", id);
        return await ValueTask.FromResult(device);
    }
}
