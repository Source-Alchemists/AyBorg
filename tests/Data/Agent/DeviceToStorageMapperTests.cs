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

using AyBorg.Runtime.Devices;
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
