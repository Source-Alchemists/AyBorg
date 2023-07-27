using AyBorg.SDK.Common;
using AyBorg.SDK.System.Runtime;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Plugins.MQTT.Tests;

public class MqttClientProviderTests
{
    private static readonly NullLogger<MqttClientProvider> s_nullLogger = new();
    private static readonly NullLoggerFactory s_nullLoggerFactory = new();
    private readonly Mock<ICommunicationStateProvider> _communicationStateProviderMock = new();
    private readonly MqttClientProvider _plugin;

    public MqttClientProviderTests()
    {
        _plugin = new MqttClientProvider(s_nullLogger, s_nullLoggerFactory, _communicationStateProviderMock.Object);
    }

    [Fact]
    public void Test_Properties()
    {
        // Assert
        Assert.Equal("AyBM", _plugin.Prefix);
        Assert.True(_plugin.CanCreate);
        Assert.Equal("MQTT Clients", _plugin.Name);
        Assert.Equal(2, _plugin.Categories.Count);
        Assert.Contains("Communication", _plugin.Categories);
        Assert.Contains("MQTT", _plugin.Categories);
    }

    [Fact]
    public async Task Test_CreateAsync()
    {
        // Act
        IDevice result = await _plugin.CreateAsync("123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result.Id);
        Assert.Equal("MQTT Client (123)", result.Name);

    }
}
