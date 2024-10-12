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

using AyBorg.Data.Agent;
using AyBorg.Runtime.Devices;
using AyBorg.Types;
using AyBorg.Types.Models;
using AyBorg.Types.Ports;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Agent.Services.Tests;

public class DeviceProxyManagerServiceTests
{
    private static readonly NullLogger<DeviceProxyManagerService> s_nullLogger = new();
    private readonly Mock<IPluginsService> _pluginServiceMock = new();
    private readonly Mock<IDeviceToStorageMapper> _deviceToStorageMapperMock = new();
    private readonly Mock<IDeviceRepository> _deviceRepositoryMock = new();
    private readonly Mock<IRuntimeConverterService> _runtimeConverterServiceMock = new();
    private readonly Mock<IDeviceProviderProxy> _deviceProviderProxyMock = new();
    private readonly Mock<IDeviceProxy> _deviceProxyMock = new();
    private readonly Mock<IDevice> _deviceMock = new();
    private readonly Mock<IPort> _portMock = new();
    private readonly Guid _portId = Guid.NewGuid();
    private readonly DeviceRecord _deviceRecord = new()
    {
        Id = "123",
        Name = "TestDevice",
        IsActive = true,
        ProviderMetaInfo = new PluginMetaInfoRecord()
    };

    private readonly DeviceProxyManagerService _service;

    public DeviceProxyManagerServiceTests()
    {
        _portMock.Setup(m => m.Id).Returns(_portId);

        _deviceMock.Setup(m => m.Id).Returns("123");
        _deviceMock.Setup(m => m.Ports).Returns(new List<IPort> { _portMock.Object });

        _deviceProxyMock.Setup(m => m.Id).Returns("123");
        _deviceProxyMock.Setup(m => m.IsActive).Returns(true);
        _deviceProxyMock.Setup(m => m.Native).Returns(_deviceMock.Object);
        _deviceProxyMock.Setup(m => m.TryConnectAsync()).ReturnsAsync(true);

        _deviceRepositoryMock.Setup(m => m.GetAllAsync()).ReturnsAsync(Array.Empty<DeviceRecord>());

        _deviceProviderProxyMock.Setup(m => m.AddAsync(It.IsAny<AddDeviceOptions>())).ReturnsAsync(_deviceProxyMock.Object);
        _deviceProviderProxyMock.Setup(m => m.Name).Returns("TestProvider");

        _pluginServiceMock.Setup(m => m.DeviceProviders).Returns(new List<IDeviceProviderProxy> { _deviceProviderProxyMock.Object });

        _deviceToStorageMapperMock.Setup(m => m.Map(It.IsAny<IDeviceProxy>())).Returns(_deviceRecord);

        _service = new DeviceProxyManagerService(s_nullLogger, _pluginServiceMock.Object, _deviceToStorageMapperMock.Object, _deviceRepositoryMock.Object, _runtimeConverterServiceMock.Object);
    }

    [Theory]
    [InlineData(true, true, true, true, true)]
    [InlineData(false, false, true, true, true)]
    [InlineData(false, true, false, true, true)]
    [InlineData(true, true, true, false, true)]
    [InlineData(false, true, true, true, false)]
    public async Task Test_LoadAsync(bool expectedResult, bool repositoyHasDevices, bool hasProvider, bool hasProviderDevices, bool canProviderAdd)
    {
        // Arrange
        bool isCalled = false;
        CollectionChangedEventArgs eventArgs = null!;
        _service.DeviceCollectionChanged += (s, e) =>
        {
            isCalled = true;
            eventArgs = e;
        };

        if (repositoyHasDevices)
        {
            _deviceRepositoryMock.Setup(m => m.GetAllAsync()).ReturnsAsync(new List<DeviceRecord> {
                _deviceRecord
            });
        }

        _deviceProviderProxyMock.Setup(m => m.Devices).Returns(hasProviderDevices ? new List<IDeviceProxy> {
            _deviceProxyMock.Object
        } : new List<IDeviceProxy>());

        _deviceProviderProxyMock.Setup(m => m.CanAdd).Returns(canProviderAdd);

        _pluginServiceMock.Setup(m => m.FindDeviceProvider(It.IsAny<PluginMetaInfoRecord>())).Returns(hasProvider ? _deviceProviderProxyMock.Object : null);

        // Act
        await _service.LoadAsync();
        await Wait(isCalled);

        // Assert
        Assert.True(isCalled);
        if (expectedResult)
        {
            Assert.Single(eventArgs.AddedItems);
        }
        else
        {
            Assert.Empty(eventArgs.AddedItems);
        }
    }

    [Theory]
    [InlineData(true, true, false, true)]
    [InlineData(false, false, false, true)]
    [InlineData(false, true, true, true)]
    [InlineData(false, true, false, false)]
    public async Task Test_AddAsync(bool expectedResult, bool knownProvider, bool deviceAlreadyExists, bool repositorySuccess)
    {
        // Arrange
        bool isCalled = false;
        CollectionChangedEventArgs eventArgs = null!;
        _service.DeviceCollectionChanged += (s, e) =>
        {
            isCalled = true;
            eventArgs = e;
        };
        _deviceProviderProxyMock.Setup(m => m.Devices).Returns(deviceAlreadyExists ? new List<IDeviceProxy> {
            _deviceProxyMock.Object
        } : new List<IDeviceProxy>());

        if (repositorySuccess)
        {
            _deviceRepositoryMock.Setup(m => m.AddAsync(It.IsAny<DeviceRecord>())).ReturnsAsync(_deviceRecord);
        }
        else
        {
            _deviceRepositoryMock.Setup(m => m.AddAsync(It.IsAny<DeviceRecord>())).ThrowsAsync(new Exception());
        }

        // Act
        if (expectedResult)
        {
            IDeviceProxy result = await _service.AddAsync(new AddDeviceOptions(knownProvider ? "TestProvider" : string.Empty, "123"));
            await Wait(isCalled);

            Assert.NotNull(result);
            Assert.Single(eventArgs.AddedItems);
        }
        else
        {
            await Assert.ThrowsAnyAsync<Exception>(async () => await _service.AddAsync(new AddDeviceOptions(knownProvider ? "TestProvider" : string.Empty, "123")));
            Assert.False(isCalled);
        }
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task Test_RemoveAsync(bool expectedResult, bool deviceExists)
    {
        // Arrange
        bool isCalled = false;
        CollectionChangedEventArgs eventArgs = null!;
        _service.DeviceCollectionChanged += (s, e) =>
        {
            isCalled = true;
            eventArgs = e;
        };
        _deviceProviderProxyMock.Setup(m => m.Devices).Returns(deviceExists ? new List<IDeviceProxy> {
            _deviceProxyMock.Object
        } : new List<IDeviceProxy>());

        _deviceProviderProxyMock.Setup(m => m.RemoveAsync(It.IsAny<string>())).ReturnsAsync(_deviceProxyMock.Object);
        _deviceRepositoryMock.Setup(m => m.RemoveAsync(It.IsAny<DeviceRecord>())).ReturnsAsync(_deviceRecord);

        // Act
        if (expectedResult)
        {
            IDeviceProxy result = await _service.RemoveAsync("123");
            await Wait(isCalled);

            // Assert
            Assert.NotNull(result);
            Assert.Single(eventArgs.RemovedItems);
        }
        else
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.RemoveAsync("123"));
            Assert.False(isCalled);
        }
    }

    private static async Task Wait(bool isCalled)
    {
        int count = 0;
        while (!isCalled && count < 10)
        {
            await Task.Delay(10);
            count++;
        }
    }

    [Theory]
    [InlineData(true, true, true, true, true, false)]
    [InlineData(true, false, true, true, true, true)]
    [InlineData(false, true, true, true, true, true)]
    [InlineData(false, false, true, true, true, false)]
    [InlineData(false, false, true, false, true, true)]
    [InlineData(false, true, true, true, false, false)]
    public async Task Test_ChangeStateAsync(bool expectedResult, bool deviceIsActive, bool deviceIsConnected, bool deviceCanConnect, bool deviceCanDisconnect, bool optionActivate)
    {
        // Arrange
        bool isCalled = false;
        _service.DeviceCollectionChanged += (s, e) =>
        {
            isCalled = true;
        };
        _deviceProviderProxyMock.Setup(m => m.Devices).Returns(new List<IDeviceProxy> {
            _deviceProxyMock.Object
        });

        _deviceProxyMock.Setup(m => m.IsActive).Returns(deviceIsActive);
        _deviceProxyMock.Setup(m => m.IsConnected).Returns(deviceIsConnected);
        _deviceProxyMock.Setup(m => m.TryConnectAsync()).ReturnsAsync(deviceCanConnect);
        _deviceProxyMock.Setup(m => m.TryDisconnectAsync()).ReturnsAsync(deviceCanDisconnect);

        // Act
        IDeviceProxy result = await _service.ChangeStateAsync(new ChangeDeviceStateOptions("123", optionActivate));

        // Assert
        if (expectedResult)
        {
            while (!isCalled)
            {
                await Task.Delay(10);
            }
            Assert.NotNull(result);
            _deviceRepositoryMock.Verify(m => m.UpdateAsync(It.IsAny<DeviceRecord>()), Times.Once);

            if (deviceCanConnect && !deviceIsActive)
            {
                _deviceProxyMock.Verify(m => m.TryConnectAsync(), Times.Once);
                _deviceProxyMock.Verify(m => m.TryDisconnectAsync(), Times.Never);
            }

            if (deviceCanDisconnect && deviceIsActive)
            {
                _deviceProxyMock.Verify(m => m.TryConnectAsync(), Times.Never);
                _deviceProxyMock.Verify(m => m.TryDisconnectAsync(), Times.Once);
            }
        }
        else
        {
            Assert.False(isCalled);
            _deviceRepositoryMock.Verify(m => m.UpdateAsync(It.IsAny<DeviceRecord>()), Times.Never);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Test_UpdateAsync(bool canUpdateRuntime)
    {
        // Arrange
        _deviceProviderProxyMock.Setup(m => m.Devices).Returns(new List<IDeviceProxy> {
            _deviceProxyMock.Object
        });
        _runtimeConverterServiceMock.Setup(m => m.TryUpdatePortValueAsync(It.IsAny<IPort>(), It.IsAny<object>())).ReturnsAsync(canUpdateRuntime);

        // Act
        IDeviceProxy result = await _service.UpdateAsync(new UpdateDeviceOptions("123", new List<PortModel> {
            new PortModel {
                Id = _portId
            }
        }));

        // Assert
        _runtimeConverterServiceMock.Verify(m => m.TryUpdatePortValueAsync(It.IsAny<IPort>(), It.IsAny<object>()), Times.Once);
        _deviceRepositoryMock.Verify(m => m.UpdateAsync(It.IsAny<DeviceRecord>()), Times.Once);
    }
}
