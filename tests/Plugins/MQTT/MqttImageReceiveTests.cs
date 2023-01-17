using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication.MQTT;
using ImageTorque;
using ImageTorque.Buffers;
using ImageTorque.Pixels;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Plugins.MQTT.Tests;

public class MqttImageReceiveTests
{
    private readonly NullLogger<MqttImageReceive> _logger = new();
    private readonly Mock<IMqttClientProvider> _mockMqttClientProvider = new();
    private readonly MqttImageReceive _mqttReceive;

    public MqttImageReceiveTests()
    {
        _mqttReceive = new MqttImageReceive(_logger, _mockMqttClientProvider.Object);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async ValueTask Test_ReceiveSubscription(bool hasPayload)
    {
        // Arrange
        var topicPort = (StringPort)_mqttReceive.Ports.First(p => p.Name.Equals("Topic"));
        var imagePort = (ImagePort)_mqttReceive.Ports.First(p => p.Name.Equals("Image"));
        var subscription = new MqttSubscription { TopicFilter = "Test" };
        _mockMqttClientProvider.Setup(m => m.SubscribeAsync(topicPort.Value)).ReturnsAsync(subscription);
        using var sendImage = new Image(new PixelBuffer<L8>(2, 2));

        // Act
        await _mqttReceive.OnInitializeAsync();
        using MemoryStream sendStream = new();
        sendImage.Save(sendStream, ImageTorque.Processing.EncoderType.Bmp);

        ValueTask<bool> runTask = _mqttReceive.TryRunAsync(default);
        subscription.MessageReceived!.Invoke(new MQTTnet.MqttApplicationMessage
        {
            Payload = hasPayload ? sendStream.ToArray() : null
        });
        bool result = await runTask;

        // Assert
        Assert.True(result);
        if(hasPayload)
        {
            Assert.NotNull(imagePort.Value);
        } else {
            Assert.Null(imagePort.Value);
        }
    }
}
