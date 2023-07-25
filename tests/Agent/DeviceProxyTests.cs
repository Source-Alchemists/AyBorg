using AyBorg.SDK.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Agent.Tests;

public class DeviceProxyTests
{
    private static readonly NullLogger<DeviceProxy> s_nullLogger = new();
    private readonly Mock<IDeviceProvider> _deviceProviderMock = new();
    private readonly Mock<IDevice> _deviceMock = new();

    [Fact]
    public void Test_Constructor()
    {
        // Act
        DeviceProxy deviceProxy = new(s_nullLogger, _deviceProviderMock.Object, _deviceMock.Object, true);

        // Assert
        Assert.NotNull(deviceProxy.Native);
        Assert.True(deviceProxy.IsActive);
        Assert.NotNull(deviceProxy.MetaInfo);
        Assert.Equal("IDeviceProxy", deviceProxy.MetaInfo.TypeName);
        Assert.NotNull(deviceProxy.ProviderMetaInfo);
        Assert.Equal("IDeviceProviderProxy", deviceProxy.ProviderMetaInfo.TypeName);
    }

    [Fact]
    public async Task Test_TryConnectAsync()
    {
        // Arrange
        _deviceMock.Setup(m => m.TryConnectAsync()).ReturnsAsync(true);
        DeviceProxy deviceProxy = new(s_nullLogger, _deviceProviderMock.Object, _deviceMock.Object, false);

        // Act
        bool result = await deviceProxy.TryConnectAsync();

        // Assert
        Assert.True(result);
        Assert.True(deviceProxy.IsActive);
    }

    [Fact]
    public async Task Test_TryDisconnectAsync()
    {
        // Arrange
        _deviceMock.Setup(m => m.TryDisconnectAsync()).ReturnsAsync(true);
        DeviceProxy deviceProxy = new(s_nullLogger, _deviceProviderMock.Object, _deviceMock.Object, true);

        // Act
        bool result = await deviceProxy.TryDisconnectAsync();

        // Assert
        Assert.True(result);
        Assert.False(deviceProxy.IsActive);
    }

}
