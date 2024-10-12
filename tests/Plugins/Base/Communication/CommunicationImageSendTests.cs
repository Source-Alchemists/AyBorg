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

using AyBorg.Communication;
using AyBorg.Types;
using AyBorg.Types.Communication;
using AyBorg.Types.Ports;

using ImageTorque;
using ImageTorque.Buffers;
using ImageTorque.Pixels;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Plugins.Base.Communication.Tests;

public class CommunicationImageSendTests
{
    private static readonly NullLogger<CommunicationImageSend> s_nullLogger = new();
    private readonly Mock<IDeviceManager> _deviceManagerMock = new();
    private readonly Mock<ICommunicationDevice> _deviceMock = new();
    private readonly Mock<ICommunicationStateProvider> _communicationStateProviderMock = new();

    public CommunicationImageSendTests()
    {

        _deviceMock.Setup(m => m.Id).Returns("1");
        _deviceMock.Setup(m => m.Name).Returns("MockDevice");

        _deviceManagerMock.Setup(m => m.GetDevice<ICommunicationDevice>("1")).Returns(_deviceMock.Object);
    }

    [Theory]
    [InlineData(false, true, false, false, false)]
    [InlineData(true, false, false, false, false)]
    [InlineData(true, true, true, true, false)]
    [InlineData(true, true, true, true, true)]
    public async Task Test_TryRunAsync(bool expectedResult, bool sendAllowed, bool hasDevice, bool deviceCanSend, bool parallel)
    {
        // Arrange
        using var plugin = new CommunicationImageSend(s_nullLogger, _deviceManagerMock.Object, _communicationStateProviderMock.Object);
        _communicationStateProviderMock.Setup(m => m.IsResultCommunicationEnabled).Returns(sendAllowed);
        _deviceMock.Setup(m => m.TrySendAsync(It.IsAny<string>(), It.IsAny<IPort>())).ReturnsAsync(deviceCanSend);

        var parallelPort = (BooleanPort)plugin.Ports.First(p => p.Name.Equals("Parallel"));
        parallelPort.Value = parallel;

        if (hasDevice)
        {
            var devicePort = (SelectPort)plugin.Ports.First(p => p.Name.Equals("Device"));
            devicePort.Value = new SelectPort.ValueContainer("1", new List<string> { "1" });
            _deviceManagerMock.Setup(m => m.GetDevices<ICommunicationDevice>()).Returns(new List<ICommunicationDevice> { _deviceMock.Object });
        }

        using var testImage = new Image(new PixelBuffer<L8>(2, 2, new L8[] { 0x00, 0x01, 0x80, 0xFF }));
        var valuePort = (ImagePort)plugin.Ports.First(p => p.Name.Equals("Value"));
        valuePort.Value = testImage;

        // Act
        // Simulate lifecycle
        await plugin.AfterInitializedAsync();
        await plugin.BeforeStartAsync();
        bool result = await plugin.TryRunAsync(CancellationToken.None);
        if(parallel)
        {
            result = await plugin.TryRunAsync(CancellationToken.None);
        }

        // Assert
        Assert.Equal(expectedResult, result);
    }
}
