using AyBorg.Gateway.Models;
using AyBorg.Gateway.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace AyBorg.Gateway.Tests.Services;

public class GrpcChannelServiceTests
{
    private static readonly NullLogger<GrpcChannelService> s_logger = new();

    private readonly GrpcChannelService _service;

    public GrpcChannelServiceTests()
    {
        _service = new GrpcChannelService(s_logger);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Test_TryRegisterChannel(bool isRegistered)
    {
        // Arrange
        string uniqueServiceName = "UniqueServiceName";
        string typeName = "TypeName";
        string address = "http://0.0.0.0:5000";

        // Act
        bool result = _service.TryRegisterChannel(uniqueServiceName, typeName, address);
        if(isRegistered)
        {
            result = _service.TryRegisterChannel(uniqueServiceName, typeName, address);
        }

        // Assert
        Assert.Equal(!isRegistered, result);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Test_TryUnregisterChannel(bool isRegistered)
    {
        // Arrange
        string uniqueServiceName = "UniqueServiceName";
        string typeName = "TypeName";
        string address = "http://0.0.0.0:5000";
        if(isRegistered)
        {
            _ = _service.TryRegisterChannel(uniqueServiceName, typeName, address);
        }

        // Act
        bool result = _service.TryUnregisterChannel(uniqueServiceName);

        // Assert
        Assert.Equal(isRegistered, result);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Test_GetChannelByName(bool isRegistered)
    {
        // Arrange
        string uniqueServiceName = "UniqueServiceName";
        string typeName = "TypeName";
        string address = "http://0.0.0.0:5000";
        if(isRegistered)
        {
            _ = _service.TryRegisterChannel(uniqueServiceName, typeName, address);
        }

        // Act
        if(!isRegistered)
        {
            Assert.Throws<KeyNotFoundException>(() => _service.GetChannelByName(uniqueServiceName));
            return;
        }

        ChannelInfo result = null!;
        result = _service.GetChannelByName(uniqueServiceName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(uniqueServiceName, result.ServiceUniqueName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Test_GetChannelsByTypeName(bool isRegistered)
    {
        // Arrange
        string uniqueServiceName = "UniqueServiceName";
        string typeName = "TypeName";
        string address = "http://0.0.0.0:5000";
        if(isRegistered)
        {
            _ = _service.TryRegisterChannel(uniqueServiceName, typeName, address);
        }
        _ = _service.TryRegisterChannel("test2", "test", "http://0.0.0.0:5001");

        // Act
        IEnumerable<ChannelInfo> result = _service.GetChannelsByTypeName(typeName);

        // Assert
        Assert.Equal(isRegistered ? 1 : 0, result.Count());
        if(isRegistered)
        {
        Assert.Equal(uniqueServiceName, result.First().ServiceUniqueName);
        }
    }
}
