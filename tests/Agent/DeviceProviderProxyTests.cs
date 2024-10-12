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
using AyBorg.Types;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Agent.Tests;

public class DeviceProviderProxyTests
{
    private static readonly NullLogger<DeviceProviderProxy> s_nullLogger = new();
    private static readonly NullLoggerFactory s_nullLoggerFactory = new();
    private readonly Mock<ITestProvider> _deviceProviderMock = new();
    private readonly Mock<IDevice> _deviceMock = new();

    [Fact]
    public void Test_Constructor()
    {
        // Arrange
        _deviceProviderMock.Setup(m => m.Name).Returns("TestDeviceProvider");
        _deviceProviderMock.Setup(m => m.Prefix).Returns("TestPrefix");
        _deviceProviderMock.Setup(m => m.CanCreate).Returns(true);

        // Act
        using var deviceProviderProxy = new DeviceProviderProxy(s_nullLoggerFactory, s_nullLogger, _deviceProviderMock.Object);

        // Assert
        Assert.NotNull(deviceProviderProxy.MetaInfo);
        Assert.Equal("ITestProviderProxy", deviceProviderProxy.MetaInfo.TypeName);
        Assert.Equal("TestDeviceProvider", deviceProviderProxy.Name);
        Assert.Equal("TestPrefix", deviceProviderProxy.Prefix);
        Assert.True(deviceProviderProxy.CanAdd);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task Test_TryInitailizeAsync(bool expectedResult, bool success)
    {
        // Arrange
        using var deviceProviderProxy = new DeviceProviderProxy(s_nullLoggerFactory, s_nullLogger, _deviceProviderMock.Object);
        if (!success)
        {
            _deviceProviderMock.Setup(m => m.AfterInitializedAsync()).Throws(new Exception());
        }

        // Act
        bool result = await deviceProviderProxy.TryInitializeAsync();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    public async Task Test_AddAsync(bool expectedResult, bool canCreate, bool addOnlyOnce)
    {
        // Arrange
        using var deviceProviderProxy = new DeviceProviderProxy(s_nullLoggerFactory, s_nullLogger, _deviceProviderMock.Object);
        _deviceProviderMock.Setup(m => m.CanCreate).Returns(canCreate);
        _deviceProviderMock.Setup(m => m.CreateAsync("123")).ReturnsAsync(_deviceMock.Object);
        _deviceMock.Setup(m => m.Id).Returns("123");

        // Act
        if (!canCreate)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await deviceProviderProxy.AddAsync(It.IsAny<AddDeviceOptions>()));
        }

        if (!addOnlyOnce)
        {
            await deviceProviderProxy.AddAsync(new AddDeviceOptions(string.Empty, "123", false));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await deviceProviderProxy.AddAsync(new AddDeviceOptions(string.Empty, "123", false)));
        }

        if (expectedResult)
        {
            IDeviceProxy result = await deviceProviderProxy.AddAsync(new AddDeviceOptions(string.Empty, "123", false));

            // Assert
            Assert.NotNull(result);
            Assert.Equal("123", result.Id);
        }
    }

    [Theory]
    [InlineData(true, true, true, true)]
    [InlineData(false, false, true, true)]
    [InlineData(false, true, true, false)]
    [InlineData(true, true, false, true)]
    public async Task Test_RemoveAsync(bool expectedResult, bool hasDevice, bool isConnected, bool canDisconnect)
    {
        // Arrange
        using var deviceProviderProxy = new DeviceProviderProxy(s_nullLoggerFactory, s_nullLogger, _deviceProviderMock.Object);
        _deviceProviderMock.Setup(m => m.CanCreate).Returns(true);
        _deviceProviderMock.Setup(m => m.CreateAsync("123")).ReturnsAsync(_deviceMock.Object);
        _deviceMock.Setup(m => m.Id).Returns("123");
        _deviceMock.Setup(m => m.IsConnected).Returns(isConnected);
        _deviceMock.Setup(m => m.TryDisconnectAsync()).ReturnsAsync(canDisconnect);

        // Act
        if (hasDevice)
        {
            await deviceProviderProxy.AddAsync(new AddDeviceOptions(string.Empty, "123", true));
        }
        else
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await deviceProviderProxy.RemoveAsync("123"));
        }

        if (!canDisconnect && isConnected)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await deviceProviderProxy.RemoveAsync("123"));
        }

        if (expectedResult)
        {
            IDeviceProxy result = await deviceProviderProxy.RemoveAsync("123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("123", result.Id);
        }
    }

    public interface ITestProvider : IDeviceProvider, IAfterInitialized, IDisposable { }
}
