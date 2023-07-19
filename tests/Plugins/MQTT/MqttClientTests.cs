using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication;
using AyBorg.SDK.Communication.MQTT;
using AyBorg.SDK.System.Runtime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Plugins.MQTT.Tests
{
    public class MqttClientTests
    {
        private static readonly NullLogger<ICommunicationDevice> s_logger = new();
        private readonly Mock<IMqttClientProviderFactory> _clientProviderFactoryMock = new();
        private readonly Mock<IMqttClientProvider> _clientProviderMock = new();
        private readonly Mock<ICommunicationStateProvider> _communicationStateProviderMock = new();

        public MqttClientTests() {
            _clientProviderFactoryMock.Setup(f => f.Create(It.IsAny<ILogger>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(_clientProviderMock.Object);
            _communicationStateProviderMock.Setup(m => m.IsResultCommunicationEnabled).Returns(true);
        }

        [Fact]
        public async Task TryConnectAsync_ShouldConnectToMqttBroker()
        {
            // Arrange
            using var mqttClient = new MqttClient(s_logger, _clientProviderFactoryMock.Object, _communicationStateProviderMock.Object, "mqtt-client");

            // Act
            bool result = await mqttClient.TryConnectAsync();

            // Assert
            _clientProviderFactoryMock.Verify(f => f.Create(It.IsAny<ILogger>(), "MQTT-Client (mqtt-client)", It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            _clientProviderMock.Verify(p => p.ConnectAsync(), Times.Once);
            Assert.True(result);
        }

        [Fact]
        public async Task TrySendAsync_ShouldPublishMqttMessage()
        {
            // Arrange
            using var mqttClient = new MqttClient(s_logger, _clientProviderFactoryMock.Object, _communicationStateProviderMock.Object, "mqtt-client");
            string messageId = "test-message";
            var portMock = new Mock<IPort>();

            // Act
            Assert.True(await mqttClient.TryConnectAsync());
            bool result = await mqttClient.TrySendAsync(messageId, portMock.Object);

            // Assert
            _clientProviderMock.Verify(p => p.PublishAsync(messageId, portMock.Object, It.IsAny<MqttPublishOptions>()), Times.Once);
            Assert.True(result);
        }

        [Fact]
        public async Task SubscribeAsync_ShouldSubscribeToMqttTopic()
        {
            // Arrange
            var rawSub = new MqttSubscription();
            _clientProviderMock.Setup(m => m.SubscribeAsync(It.IsAny<string>())).ReturnsAsync(rawSub);
            using var mqttClient = new MqttClient(s_logger, _clientProviderFactoryMock.Object, _communicationStateProviderMock.Object, "mqtt-client");
            string messageId = "test-message";

            // Act
            Assert.True(await mqttClient.TryConnectAsync());
            IMessageSubscription subscription = await mqttClient.SubscribeAsync(messageId);

            // Assert
            _clientProviderMock.Verify(p => p.SubscribeAsync(messageId), Times.Once);
            Assert.NotNull(subscription);
        }

        [Fact]
        public async Task UnsubscribeAsync_ShouldUnsubscribeFromMqttTopic()
        {
            // Arrange
            var rawSub = new MqttSubscription();
            _clientProviderMock.Setup(m => m.SubscribeAsync(It.IsAny<string>())).ReturnsAsync(rawSub);
            using var mqttClient = new MqttClient(s_logger, _clientProviderFactoryMock.Object, _communicationStateProviderMock.Object, "mqtt-client");
            string messageId = "test-message";
            var subscriptionMock = new Mock<IMessageSubscription>();
            subscriptionMock.Setup(m => m.Id).Returns(messageId);

            // Act
            Assert.True(await mqttClient.TryConnectAsync());
            Assert.NotNull(await mqttClient.SubscribeAsync(messageId));
            await mqttClient.UnsubscribeAsync(subscriptionMock.Object);

            // Assert
            _clientProviderMock.Verify(p => p.UnsubscribeAsync(It.IsAny<MqttSubscription>()), Times.Once);
        }
    }
}
