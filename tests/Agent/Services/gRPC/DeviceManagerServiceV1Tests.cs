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

using AyBorg.Agent.Tests.Services.gRPC;
using AyBorg.Communication.gRPC;
using AyBorg.Runtime;
using AyBorg.Runtime.Devices;
using AyBorg.Types;
using AyBorg.Types.Models;
using AyBorg.Types.Ports;
using Moq;

namespace AyBorg.Agent.Services.gRPC.Tests;

public class DeviceManagerServiceV1Tests : BaseGrpcServiceTests<DeviceManagerServiceV1, Ayborg.Gateway.Agent.V1.DeviceManager.DeviceManagerClient>
{
    private readonly Mock<IDeviceProxyManagerService> _deviceManagerServiceMock = new();
    private readonly Mock<IRuntimeMapper> _runtimeMapperMock = new();
    private readonly Mock<IRpcMapper> _rpcMapperMock = new();
    private readonly Mock<IDeviceProviderProxy> _deviceProviderProxyMock = new();
    private readonly Mock<IDeviceProxy> _deviceProxyMock = new();
    private readonly Mock<IDevice> _deviceMock = new();
    private readonly StringPort _testPort;

    public DeviceManagerServiceV1Tests()
    {
        _testPort = new StringPort("TestPort", PortDirection.Input, "TestValue");

        _deviceMock.Setup(m => m.Ports).Returns(new List<IPort> { _testPort });

        _deviceProxyMock.Setup(m => m.Id).Returns("TestPrefix-123");
        _deviceProxyMock.Setup(m => m.Name).Returns("TestDevice");
        _deviceProxyMock.Setup(m => m.Manufacturer).Returns("TestManufacturer");
        _deviceProxyMock.Setup(m => m.IsActive).Returns(true);
        _deviceProxyMock.Setup(m => m.IsConnected).Returns(true);
        _deviceProxyMock.Setup(m => m.Categories).Returns(new List<string> { "TestCategory" });
        _deviceProxyMock.Setup(m => m.Native).Returns(_deviceMock.Object);

        _deviceProviderProxyMock.Setup(m => m.Name).Returns("TestProvider");
        _deviceProviderProxyMock.Setup(m => m.Prefix).Returns("TestPrefix-");
        _deviceProviderProxyMock.Setup(m => m.CanAdd).Returns(true);
        _deviceProviderProxyMock.Setup(m => m.Devices).Returns(new List<IDeviceProxy> { _deviceProxyMock.Object });

        _deviceManagerServiceMock.Setup(m => m.DeviceProviders).Returns(new List<IDeviceProviderProxy> { _deviceProviderProxyMock.Object });
        _deviceManagerServiceMock.Setup(m => m.AddAsync(It.IsAny<AddDeviceOptions>())).ReturnsAsync(_deviceProxyMock.Object);
        _deviceManagerServiceMock.Setup(m => m.RemoveAsync(It.IsAny<string>())).ReturnsAsync(_deviceProxyMock.Object);
        _deviceManagerServiceMock.Setup(m => m.ChangeStateAsync(It.IsAny<ChangeDeviceStateOptions>())).ReturnsAsync(_deviceProxyMock.Object);
        _deviceManagerServiceMock.Setup(m => m.UpdateAsync(It.IsAny<UpdateDeviceOptions>())).ReturnsAsync(_deviceProxyMock.Object);


        _rpcMapperMock.Setup(m => m.ToRpc(It.IsAny<PortModel>())).Returns(new Ayborg.Gateway.Agent.V1.PortDto
        {
            Id = "123",
            Name = "TestPort",
            Direction = (int)PortDirection.Input,
            Value = "TestValue"
        });

        _service = new DeviceManagerServiceV1(_deviceManagerServiceMock.Object, _runtimeMapperMock.Object, _rpcMapperMock.Object);
    }

    [Fact]
    public async Task Test_GetAvailableProviders()
    {
        // Act
        Ayborg.Gateway.Agent.V1.DeviceProviderCollectionResponse response = await _service.GetAvailableProviders(new Ayborg.Gateway.Agent.V1.DefaultAgentRequest
        {
            AgentUniqueName = "123"
        }, _serverCallContext);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.DeviceProviders);
        Assert.Equal("TestProvider", response.DeviceProviders[0].Name);
        Assert.Equal("TestPrefix", response.DeviceProviders[0].Prefix);
        Assert.True(response.DeviceProviders[0].CanAdd);
        Assert.Single(response.DeviceProviders[0].Devices);
        Assert.Equal("TestPrefix-123", response.DeviceProviders[0].Devices[0].Id);
        Assert.Equal("TestDevice", response.DeviceProviders[0].Devices[0].Name);
        Assert.Equal("TestManufacturer", response.DeviceProviders[0].Devices[0].Manufacturer);
        Assert.True(response.DeviceProviders[0].Devices[0].IsActive);
    }

    [Fact]
    public async Task Test_GetDevice()
    {
        // Act
        Ayborg.Gateway.Agent.V1.DeviceDto response = await _service.GetDevice(new Ayborg.Gateway.Agent.V1.GetDeviceRequest
        {
            DeviceId = "TestPrefix-123"
        }, _serverCallContext);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("TestPrefix-123", response.Id);
        Assert.Equal("TestDevice", response.Name);
        Assert.Equal("TestManufacturer", response.Manufacturer);
        Assert.True(response.IsActive);
        Assert.True(response.IsConnected);
        Assert.Single(response.Categories);
        Assert.Equal("TestCategory", response.Categories[0]);
        Assert.Single(response.Ports);
        Assert.Equal("TestPort", response.Ports[0].Name);
        Assert.Equal("TestValue", response.Ports[0].Value);
    }

    [Fact]
    public async Task Test_Add()
    {
        // Act
        Ayborg.Gateway.Agent.V1.DeviceDto response = await _service.Add(new Ayborg.Gateway.Agent.V1.AddDeviceRequest
        {
            DeviceId = "123",
            DeviceProviderName = "TestProvider",
            DevicePrefix = "TestPrefix-"
        }, _serverCallContext);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("TestPrefix-123", response.Id);
        Assert.Equal("TestDevice", response.Name);
        Assert.Equal("TestManufacturer", response.Manufacturer);
        Assert.True(response.IsActive);
        Assert.True(response.IsConnected);
        Assert.Single(response.Categories);
        Assert.Equal("TestCategory", response.Categories[0]);
    }

    [Fact]
    public async Task Test_Remove()
    {
        // Act
        Ayborg.Gateway.Agent.V1.DeviceDto response = await _service.Remove(new Ayborg.Gateway.Agent.V1.RemoveDeviceRequest
        {
            DeviceId = "123",
        }, _serverCallContext);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("TestPrefix-123", response.Id);
        Assert.Equal("TestDevice", response.Name);
        Assert.Equal("TestManufacturer", response.Manufacturer);
        Assert.True(response.IsActive);
        Assert.True(response.IsConnected);
        Assert.Single(response.Categories);
        Assert.Equal("TestCategory", response.Categories[0]);
    }

    [Fact]
    public async Task Test_ChangeState()
    {
        // Act
        Ayborg.Gateway.Agent.V1.DeviceDto response = await _service.ChangeState(new Ayborg.Gateway.Agent.V1.DeviceStateRequest
        {
            DeviceId = "123",
        }, _serverCallContext);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("TestPrefix-123", response.Id);
        Assert.Equal("TestDevice", response.Name);
        Assert.Equal("TestManufacturer", response.Manufacturer);
        Assert.True(response.IsActive);
        Assert.True(response.IsConnected);
        Assert.Single(response.Categories);
        Assert.Equal("TestCategory", response.Categories[0]);
    }

    [Fact]
    public async Task Test_UpdateDevice()
    {
        // Arrange
        var request = new Ayborg.Gateway.Agent.V1.UpdateDeviceRequest
        {
            DeviceId = "123"
        };
        request.Ports.Add(new Ayborg.Gateway.Agent.V1.PortDto {});

        // Act
        Ayborg.Gateway.Agent.V1.DeviceDto response = await _service.UpdateDevice(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("TestPrefix-123", response.Id);
        Assert.Equal("TestDevice", response.Name);
        Assert.Equal("TestManufacturer", response.Manufacturer);
        Assert.True(response.IsActive);
        Assert.True(response.IsConnected);
        Assert.Single(response.Categories);
        Assert.Equal("TestCategory", response.Categories[0]);
        Assert.Single(response.Ports);
        Assert.Equal("TestPort", response.Ports[0].Name);
        Assert.Equal("TestValue", response.Ports[0].Value);
        _rpcMapperMock.Verify(m => m.FromRpc(It.IsAny<Ayborg.Gateway.Agent.V1.PortDto>()), Times.Once);
    }
}
