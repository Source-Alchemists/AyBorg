using AyBorg.SDK.Projects;
using Moq;

namespace AyBorg.Data.Agent.Tests;

public class DeviceToStorageMapperTests
{
    [Fact]
    public void Test_MapDeviceProxy()
    {
        // Arrange
        var mapper = new DeviceToStorageMapper();
        var deviceProxyMock = new Mock<IDeviceProxy>();
        deviceProxyMock.Setup(m => m.Id).Returns("123");
        deviceProxyMock.Setup(m => m.Name).Returns("Test Device");
        deviceProxyMock.Setup(m => m.Manufacturer).Returns("Test Manufacturer");
        deviceProxyMock.Setup(m => m.IsActive).Returns(true);
        deviceProxyMock.Setup(m => m.IsConnected).Returns(true);
        deviceProxyMock.Setup(m => m.Categories).Returns(new[] { "Test Category" });

        // Act
        DeviceRecord result = mapper.Map(deviceProxyMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result.Id);
        Assert.Equal("Test Device", result.Name);
        Assert.True(result.IsActive);
    }
}
