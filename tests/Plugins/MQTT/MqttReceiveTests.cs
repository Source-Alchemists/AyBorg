using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication.MQTT;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Plugins.MQTT.Tests;

public class MqttReceiveTests
{
    private readonly NullLogger<MqttReceive> _logger = new();
    private readonly Mock<IMqttClientProvider> _mockMqttClientProvider = new();
    private readonly MqttReceive _mqttReceive;

    public MqttReceiveTests()
    {
        _mqttReceive = new MqttReceive(_logger, _mockMqttClientProvider.Object);
    }

    [Theory]
    [InlineData("TestPayload")]
    [InlineData(null)]
    public async ValueTask Test_ReceiveSubscription(string? payload)
    {
        // Arrange
        var topicPort = (StringPort)_mqttReceive.Ports.First(p => p.Name.Equals("Topic"));
        var messagePort = (StringPort)_mqttReceive.Ports.First(p => p.Name.Equals("Message"));
        var subscription = new MqttSubscription { TopicFilter = "Test" };
        _mockMqttClientProvider.Setup(m => m.SubscribeAsync(topicPort.Value)).ReturnsAsync(subscription);

        // Act
        await _mqttReceive.OnInitializeAsync();
        subscription.MessageReceived!.Invoke(new MQTTnet.MqttApplicationMessage
        {
            Payload = payload != null ? System.Text.Encoding.UTF8.GetBytes(payload) : null
        });
        bool result = await _mqttReceive.TryRunAsync(default);

        // Assert
        Assert.True(result);
        Assert.Equal(payload, messagePort.Value);
    }
}
