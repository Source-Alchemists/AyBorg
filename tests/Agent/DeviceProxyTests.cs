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

using AyBorg.Types;
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
        // Arrange
        _deviceMock.Setup(m => m.Id).Returns("123");
        _deviceMock.Setup(m => m.Name).Returns("TestDevice");
        _deviceMock.Setup(m => m.Manufacturer).Returns("TestManufacturer");
        _deviceMock.Setup(m => m.IsConnected).Returns(true);

        // Act
        using DeviceProxy deviceProxy = new(s_nullLogger, _deviceProviderMock.Object, _deviceMock.Object, true);

        // Assert
        Assert.NotNull(deviceProxy.Native);
        Assert.True(deviceProxy.IsActive);
        Assert.NotNull(deviceProxy.MetaInfo);
        Assert.Equal("IDeviceProxy", deviceProxy.MetaInfo.TypeName);
        Assert.NotNull(deviceProxy.ProviderMetaInfo);
        Assert.Equal("IDeviceProviderProxy", deviceProxy.ProviderMetaInfo.TypeName);
        Assert.Equal("123", deviceProxy.Id);
        Assert.Equal("TestDevice", deviceProxy.Name);
        Assert.Equal("TestManufacturer", deviceProxy.Manufacturer);
        Assert.True(deviceProxy.IsConnected);
    }

    [Fact]
    public async Task Test_TryConnectAsync()
    {
        // Arrange
        _deviceMock.Setup(m => m.TryConnectAsync()).ReturnsAsync(true);
        using DeviceProxy deviceProxy = new(s_nullLogger, _deviceProviderMock.Object, _deviceMock.Object, false);

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
        using DeviceProxy deviceProxy = new(s_nullLogger, _deviceProviderMock.Object, _deviceMock.Object, true);

        // Act
        bool result = await deviceProxy.TryDisconnectAsync();

        // Assert
        Assert.True(result);
        Assert.False(deviceProxy.IsActive);
    }

}
