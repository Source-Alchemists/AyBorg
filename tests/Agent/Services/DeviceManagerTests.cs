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
using AyBorg.Types.Communication;
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
            _deviceProviderProxyMock.Setup(m => m.Devices).Returns([_deviceProxyMock.Object]);
        }
        else
        {
            _deviceProviderProxyMock.Setup(m => m.Devices).Returns([]);
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
