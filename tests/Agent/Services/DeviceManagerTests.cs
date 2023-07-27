using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Communication;
using AyBorg.SDK.Projects;
using Moq;

namespace AyBorg.Agent.Services.Tests;

public class DeviceManagerTests
{
    private readonly Mock<IDeviceProxyManagerService> _deviceProxyManagerServiceMock = new();
    private readonly Mock<IDeviceProviderProxy> _deviceProviderProxyMock = new();
    private readonly Mock<IDeviceProxy> _deviceProxyMock = new();
    private readonly Mock<ICommunicationDevice> _deviceMock = new();
    private readonly DeviceManager _manager;

    public DeviceManagerTests()
    {
        _deviceMock.Setup(m => m.Id).Returns("123");
        _deviceProxyMock.Setup(m => m.Id).Returns("123");
        _deviceProxyMock.Setup(m => m.Native).Returns(_deviceMock.Object);
        _deviceProxyManagerServiceMock.Setup(m => m.DeviceProviders).Returns(new List<IDeviceProviderProxy> { _deviceProviderProxyMock.Object });
        _manager = new DeviceManager(_deviceProxyManagerServiceMock.Object);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    public void Test_GetDevice(bool expectedResult, bool hasDevice, bool isDeviceActive)
    {
        // Arranage
        _deviceProxyMock.Setup(m => m.IsActive).Returns(isDeviceActive);
        if (hasDevice)
        {
            _deviceProviderProxyMock.Setup(m => m.Devices).Returns(new List<IDeviceProxy> { _deviceProxyMock.Object });
        }
        else
        {
            _deviceProviderProxyMock.Setup(m => m.Devices).Returns(new List<IDeviceProxy>());
        }

        // Act
        ICommunicationDevice result = _manager.GetDevice<ICommunicationDevice>("123");

        // Assert
        if (expectedResult)
        {
            Assert.NotNull(result);
        }
        else
        {
            Assert.Null(result);
        }
    }
}
