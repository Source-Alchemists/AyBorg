using System.Text;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Communication;
using AyBorg.SDK.Common.Ports;
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
        var messageIdPort = (StringPort)_plugin.Ports.First(p => p.Name.Equals("Id"));
        messageIdPort.Value = "TestId";
        var timeoutPort = (NumericPort)_plugin.Ports.First(p => p.Name.Equals("Timeout (ms)"));
        timeoutPort.Value = 1000;
        var valuePort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Value"));
        var testImage = Image.Load("./resources/luna.jpg");

        using var ms = new MemoryStream();
        testImage.Save(ms, ImageTorque.Processing.EncoderType.Bmp);
        ms.Position = 0;
        byte[] imageArray = ms.ToArray();

        _messageMock.Setup(m => m.Payload).Returns(hasPayload ? imageArray : null!);

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
