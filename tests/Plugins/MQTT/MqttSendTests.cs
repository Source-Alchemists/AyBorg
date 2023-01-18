using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication.MQTT;
using AyBorg.SDK.System.Runtime;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Plugins.MQTT.Tests;

public sealed class MqttSendTests
{
    private readonly NullLogger<MqttSend> _logger = new();

    private readonly Mock<IMqttClientProvider> _mqttClientProvider;
    private readonly Mock<ICommunicationStateProvider> _communicationStateProvider;

    public MqttSendTests()
    {
        _mqttClientProvider = new();
        _communicationStateProvider = new();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public async Task Test_TryRunAsync(bool IsResultCommunicationEnabled, bool expectedResult)
    {
        // Arrange
        _communicationStateProvider.Setup(x => x.IsResultCommunicationEnabled).Returns(IsResultCommunicationEnabled);
        using var mqttSend = new MqttSend(_logger, _mqttClientProvider.Object, _communicationStateProvider.Object);

        // Act
        bool result = await mqttSend.TryRunAsync(CancellationToken.None);

        // Assert
        Assert.True(expectedResult);
        if(expectedResult && IsResultCommunicationEnabled)
        {
            _mqttClientProvider.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<IPort>(), It.IsAny<MqttPublishOptions>()), Times.Once);
        }
        else
        {
            _mqttClientProvider.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<IPort>(), It.IsAny<MqttPublishOptions>()), Times.Never);
        }
    }
}
