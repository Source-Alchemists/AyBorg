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
using AyBorg.Communication.MQTT;
using AyBorg.Types.Communication;
using AyBorg.Types.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

namespace AyBorg.Plugins.MQTT.Tests;

public class MqttClientTests
{
    private static readonly NullLogger<ICommunicationDevice> s_logger = new();
    private readonly Mock<IMqttClientProviderFactory> _clientProviderFactoryMock = new();
    private readonly Mock<IMqttClientProvider> _clientProviderMock = new();
    private readonly Mock<ICommunicationStateProvider> _communicationStateProviderMock = new();

    public MqttClientTests()
    {
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
        _clientProviderFactoryMock.Verify(f => f.Create(It.IsAny<ILogger>(), "MQTT Client (mqtt-client)", It.IsAny<string>(), It.IsAny<int>()), Times.Once);
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

    [Fact]
    public async Task Test_TryUpdateAsync()
    {
        // Arrange
        using MqttClient plugin = new(s_logger, _clientProviderFactoryMock.Object, _communicationStateProviderMock.Object, "mqtt-client");
        var hostPort = (StringPort)plugin.Ports.First(p => p.Name.Equals("Host"));
        hostPort.Value = "localhost";

        var hostPortCopy = new StringPort(hostPort)
        {
            Value = "mqttbroker-1"
        };

        // Act
        bool result = await plugin.TryUpdateAsync(new List<IPort> { hostPortCopy });

        // Assert
        Assert.True(result);
        Assert.Equal("mqttbroker-1", hostPort.Value);
    }

    [Fact]
    public async Task Test_TryConnectAsync()
    {
        // Arrange
        using MqttClient plugin = new(s_logger, _clientProviderFactoryMock.Object, _communicationStateProviderMock.Object, "mqtt-client");
        var hostPort = (StringPort)plugin.Ports.First(p => p.Name.Equals("Host"));
        hostPort.Value = "localhost";

        // Act
        bool result = await plugin.TryConnectAsync();

        // Assert
        Assert.True(result);
        Assert.True(plugin.IsConnected);
    }

    [Fact]
    public async Task Test_TryDisonnectAsync()
    {
        // Arrange
        using MqttClient plugin = new(s_logger, _clientProviderFactoryMock.Object, _communicationStateProviderMock.Object, "mqtt-client");
        var hostPort = (StringPort)plugin.Ports.First(p => p.Name.Equals("Host"));
        hostPort.Value = "localhost";

        // Act
        bool result = await plugin.TryDisconnectAsync();

        // Assert
        Assert.True(result);
        Assert.False(plugin.IsConnected);
    }
}
