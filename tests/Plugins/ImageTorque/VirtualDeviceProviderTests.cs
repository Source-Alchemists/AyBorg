using AyBorg.SDK.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Plugins.ImageTorque.Tests;

public class VirtualDeviceProviderTests
{
    private static readonly NullLogger<VirtualDeviceProvider> s_nullLogger = new();
    private static readonly NullLoggerFactory s_nullLoggerFactory = new();
    private readonly Mock<IEnvironment> _environmentMock = new();
    private readonly VirtualDeviceProvider _plugin;

    public VirtualDeviceProviderTests()
    {
        _plugin = new VirtualDeviceProvider(s_nullLogger, s_nullLoggerFactory, _environmentMock.Object);
    }

    [Fact]
    public void Test_Properties()
    {
        // Assert
        Assert.Equal("AyBV", _plugin.Prefix);
        Assert.True(_plugin.CanCreate);
        Assert.Equal("Virtual Devices", _plugin.Name);
        Assert.Equal(2, _plugin.Categories.Count);
        Assert.Contains("Camera", _plugin.Categories);
        Assert.Contains("Virtual Device", _plugin.Categories);
    }

    [Fact]
    public async Task Test_CreateAsync()
    {
        // Act
        IDevice result = await _plugin.CreateAsync("123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result.Id);
        Assert.Equal("Virtual Device (123)", result.Name);
    }
}
