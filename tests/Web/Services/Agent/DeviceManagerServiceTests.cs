using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.Web.Shared.Models.Agent;
using AyBorg.Web.Tests.Helpers;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Web.Services.Agent.Tests;

public class DeviceManagerServiceTests
{
    private static readonly NullLogger<DeviceManagerService> s_nullLogger = new();
    private readonly Mock<DeviceManager.DeviceManagerClient> _deviceManagerClientMock = new();
    private readonly Mock<IRpcMapper> _rpcMapperMock = new();
    private readonly DeviceManagerService _service;

    public DeviceManagerServiceTests()
    {
        _service = new DeviceManagerService(s_nullLogger, _deviceManagerClientMock.Object, _rpcMapperMock.Object);
    }

    [Fact]
    public async Task Test_GetDeviceProvidersAsync()
    {
        // Arrange
        var response = new DeviceProviderCollectionResponse();
        var providerDto = new DeviceProviderDto
        {
            Name = "TestProvider",
            Prefix = "Ay",
            CanAdd = true
        };
        var deviceDto = new DeviceDto
        {
            Id = "123",
            Name = "TestDevice",
            Manufacturer = "SA",
            IsActive = true,
            IsConnected = true,
        };
        deviceDto.Categories.Add("Cat1");
        providerDto.Devices.Add(deviceDto);
        response.DeviceProviders.Add(providerDto);
        AsyncUnaryCall<DeviceProviderCollectionResponse> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
        _deviceManagerClientMock.Setup(m => m.GetAvailableProvidersAsync(new DefaultAgentRequest { AgentUniqueName = "Test" }, null, null, It.IsAny<CancellationToken>())).Returns(call);

        // Act
        IReadOnlyCollection<DeviceProviderMeta> result = await _service.GetDeviceProvidersAsync("Test");

        // Assert
        Assert.Single(result);
        DeviceProviderMeta rp = result.First();
        Assert.Equal("TestProvider", rp.Name);
        Assert.Equal("Ay", rp.Prefix);
        Assert.True(rp.CanAdd);
        Assert.Single(rp.Devices);
    }

    [Fact]
    public async Task Test_GetDeviceAsync()
    {
        // Arrange
        var response = new DeviceDto
        {
            Id = "123",
            Name = "TestDevice",
            Manufacturer = "SA",
            IsActive = true,
            IsConnected = true,
        };
        AsyncUnaryCall<DeviceDto> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
        _deviceManagerClientMock.Setup(m => m.GetDeviceAsync(It.IsAny<GetDeviceRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        // Act
        DeviceMeta result = await _service.GetDeviceAsync(new DeviceManagerService.CommonDeviceRequestOptions("Test", "123"));

        // Assert
        Assert.Equal("123", result.Id);
        Assert.Equal("TestDevice", result.Name);
        Assert.Equal("SA", result.Manufacturer);
        Assert.True(result.IsActive);
        Assert.True(result.IsConnected);
    }

    [Fact]
    public async Task Test_AddDeviceAsync()
    {
        // Arrange
        var response = new DeviceDto
        {
            Id = "123",
            Name = "TestDevice",
            Manufacturer = "SA",
            IsActive = true,
            IsConnected = true,
        };
        AsyncUnaryCall<DeviceDto> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
        _deviceManagerClientMock.Setup(m => m.AddAsync(It.IsAny<AddDeviceRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        // Act
        DeviceMeta result = await _service.AddDeviceAsync(new DeviceManagerService.AddDeviceRequestOptions("Test", "TestProvider", "Ay", "123"));

        // Assert
        Assert.Equal("123", result.Id);
        Assert.Equal("TestDevice", result.Name);
        Assert.Equal("SA", result.Manufacturer);
        Assert.True(result.IsActive);
        Assert.True(result.IsConnected);
    }

    [Fact]
    public async Task Test_RemoveDeviceAsync()
    {
        // Arrange
        var response = new DeviceDto
        {
            Id = "123",
            Name = "TestDevice",
            Manufacturer = "SA",
            IsActive = true,
            IsConnected = true,
        };
        AsyncUnaryCall<DeviceDto> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
        _deviceManagerClientMock.Setup(m => m.RemoveAsync(It.IsAny<RemoveDeviceRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        // Act
        DeviceMeta result = await _service.RemoveDeviceAsync(new DeviceManagerService.CommonDeviceRequestOptions("Test", "123"));

        // Assert
        Assert.Equal("123", result.Id);
        Assert.Equal("TestDevice", result.Name);
        Assert.Equal("SA", result.Manufacturer);
        Assert.True(result.IsActive);
        Assert.True(result.IsConnected);
    }

    [Fact]
    public async Task Test_ChangeDeviceStateAsync()
    {
        // Arrange
        var response = new DeviceDto
        {
            Id = "123",
            Name = "TestDevice",
            Manufacturer = "SA",
            IsActive = true,
            IsConnected = true,
        };
        AsyncUnaryCall<DeviceDto> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
        _deviceManagerClientMock.Setup(m => m.ChangeStateAsync(It.IsAny<DeviceStateRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        // Act
        DeviceMeta result = await _service.ChangeDeviceStateAsync(new DeviceManagerService.ChangeDeviceStateRequestOptions("Test", "123", true));

        // Assert
        Assert.Equal("123", result.Id);
        Assert.Equal("TestDevice", result.Name);
        Assert.Equal("SA", result.Manufacturer);
        Assert.True(result.IsActive);
        Assert.True(result.IsConnected);
    }

    [Fact]
    public async Task Test_UpdateDeviceAsync()
    {
        // Arrange
        var response = new DeviceDto
        {
            Id = "123",
            Name = "TestDevice",
            Manufacturer = "SA",
            IsActive = true,
            IsConnected = true,
        };
        AsyncUnaryCall<DeviceDto> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
        _deviceManagerClientMock.Setup(m => m.UpdateDeviceAsync(It.IsAny<UpdateDeviceRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);
        _rpcMapperMock.Setup(m => m.ToRpc(It.IsAny<Port>())).Returns(new PortDto());

        // Act
        DeviceMeta result = await _service.UpdateDeviceAsync(new DeviceManagerService.UpdateDeviceRequestOptions("Test", "123", new List<Port> {
            new Port
            {
                Name = "TestPort"
            }
        }));

        // Assert
        Assert.Equal("123", result.Id);
        Assert.Equal("TestDevice", result.Name);
        Assert.Equal("SA", result.Manufacturer);
        Assert.True(result.IsActive);
        Assert.True(result.IsConnected);
    }
}
