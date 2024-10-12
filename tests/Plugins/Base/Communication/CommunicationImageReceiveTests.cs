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
using AyBorg.Types.Communication;
using AyBorg.Types.Ports;
using ImageTorque;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Plugins.Base.Communication.Tests;

public class CommunicationImageReceiveTests : IDisposable
{
    private static readonly NullLogger<CommunicationImageReceive> s_nullLogger = new();
    private readonly Mock<IDeviceManager> _deviceManagerMock = new();
    private readonly Mock<ICommunicationDevice> _deviceMock = new();
    private readonly Mock<IMessageSubscription> _subscriptionMock = new();
    private readonly Mock<IMessage> _messageMock = new();

    private readonly CommunicationImageReceive _plugin;
    private bool _isDisposed = false;

    public CommunicationImageReceiveTests()
    {
        _deviceMock.Setup(m => m.Id).Returns("123");
        _deviceMock.Setup(m => m.SubscribeAsync(It.IsAny<string>())).ReturnsAsync(_subscriptionMock.Object);

        _deviceManagerMock.Setup(m => m.GetDevices<ICommunicationDevice>()).Returns(new List<ICommunicationDevice> { _deviceMock.Object });
        _deviceManagerMock.Setup(m => m.GetDevice<ICommunicationDevice>("123")).Returns(_deviceMock.Object);

        _plugin = new CommunicationImageReceive(s_nullLogger, _deviceManagerMock.Object);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    public async Task Test_TryRunAsync(bool expectedResult, bool hasDevice, bool hasPayload)
    {
        // Arrange
        var devicePort = (SelectPort)_plugin.Ports.First(p => p.Name.Equals("Device"));
        devicePort.Value = new SelectPort.ValueContainer(hasDevice ? "123" : string.Empty, new List<string> { "123" });
        if (!hasDevice)
        {
            _deviceManagerMock.Setup(m => m.GetDevices<ICommunicationDevice>()).Returns(Array.Empty<ICommunicationDevice>());
        }

        var messageIdPort = (StringPort)_plugin.Ports.First(p => p.Name.Equals("Id"));
        messageIdPort.Value = "TestId";
        var timeoutPort = (NumericPort)_plugin.Ports.First(p => p.Name.Equals("Timeout (ms)"));
        timeoutPort.Value = 1000;
        var valuePort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Value"));
        var testImage = Image.Load("./resources/luna.jpg");

        using var ms = new MemoryStream();
        testImage.Save(ms, "bmp");
        ms.Position = 0;
        byte[] imageArray = ms.ToArray();

        _messageMock.Setup(m => m.Payload).Returns(hasPayload ? imageArray : ArraySegment<byte>.Empty);

        // Act
        // Simulate lifecycle
        await _plugin.AfterInitializedAsync();
        if (hasDevice)
        {
            await _plugin.BeforeStartAsync();
            ValueTask<bool> rt = _plugin.TryRunAsync(CancellationToken.None);
            await Task.Delay(50);
            _subscriptionMock.Raise(m => m.Received += null, new MessageEventArgs(_messageMock.Object));
            bool result = await rt;

            // Assert
            Assert.Equal(expectedResult, result);
            if (hasPayload)
            {
                Assert.NotNull(valuePort.Value);
            }
            else
            {
                Assert.Null(valuePort.Value);
            }
        }
        else
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _plugin.BeforeStartAsync());
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
        if (isDisposing && !_isDisposed)
        {
            _plugin?.Dispose();
            _isDisposed = true;
        }
    }
}
