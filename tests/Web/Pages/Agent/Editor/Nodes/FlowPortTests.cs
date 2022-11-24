using System.Text;
using System.Text.Json;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication.MQTT;
using AyBorg.SDK.Data.DTOs;
using AyBorg.SDK.ImageProcessing;
using AyBorg.SDK.ImageProcessing.Encoding;
using AyBorg.Web.Pages.Agent.Editor.Nodes;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Services.AppState;
using AyBorg.Web.Shared.Models;
using Moq;
using MQTTnet;

namespace AyBorg.Web.Tests.Pages.Agent.Editor.Nodes;

public class FlowPortTests
{
    private readonly Mock<IFlowService> _flowServiceMock;
    private readonly Mock<IStateService> _stateServiceMock;
    private readonly Mock<IMqttClientProvider> _mqttClientProviderMock;
    private readonly Mock<IMqttClientProvider> _mqttClientProviderStepMock;
    private readonly MqttSubscription _mqttSubscription;
    private readonly MqttSubscription _mqttSubscriptionStep;

    public FlowPortTests()
    {
        _flowServiceMock = new Mock<IFlowService>();
        _stateServiceMock = new Mock<IStateService>();
        _stateServiceMock.Setup(x => x.AgentState).Returns(new UiAgentState { UniqueName = "test" });

        _mqttSubscription = new MqttSubscription { TopicFilter = "test/data" };
        _mqttSubscriptionStep = new MqttSubscription { TopicFilter = "test" };

        _mqttClientProviderMock = new Mock<IMqttClientProvider>();
        _mqttClientProviderMock.Setup(x => x.SubscribeAsync(It.IsAny<string>())).ReturnsAsync(_mqttSubscription);

        _mqttClientProviderStepMock = new Mock<IMqttClientProvider>();
        _mqttClientProviderStepMock.Setup(x => x.SubscribeAsync(It.IsAny<string>())).ReturnsAsync(_mqttSubscriptionStep);
    }

    [Theory]
    [InlineData(PortBrand.String, "test", "test string")]
    [InlineData(PortBrand.Folder, "test", "test folder")]
    [InlineData(PortBrand.Numeric, "test", "123")]
    [InlineData(PortBrand.Rectangle, "test", "{\"X\":1,\"Y\":2,\"Width\":3,\"Height\":4}")]
    [InlineData(PortBrand.Image, "test", "{\"Meta\":{\"Width\":100,\"Height\":100,\"PixelFormat\":2,\"EncoderType\":2},\"Base64\":\"1234\"}")]
    public void Test_MqttMessageReceived(PortBrand brand, string topic, string value)
    {
        // Arrange
        var step = new StepDto { Id = Guid.NewGuid(), ExecutionTimeMs = 0 };
        var port = new PortDto { Id = Guid.NewGuid(), Name = "test", Brand = brand };
        using var flowNode = new FlowNode(_flowServiceMock.Object, _mqttClientProviderStepMock.Object, _stateServiceMock.Object, step);
        using var flowPort = new FlowPort(flowNode, port, step, _flowServiceMock.Object, _mqttClientProviderMock.Object, _stateServiceMock.Object);
        bool invoked = false;
        flowPort.PortChanged += () => { invoked = true; };

        // Act
        _mqttSubscription.MessageReceived!.Invoke(new MqttApplicationMessage { Payload = Encoding.UTF8.GetBytes(value), Topic = topic});

        // Assert
        Assert.True(invoked);
        switch(brand)
        {
            case PortBrand.String:
                Assert.Equal(value, flowPort.Port.Value);
                break;
            case PortBrand.Folder:
                Assert.Equal(value, flowPort.Port.Value);
                break;
            case PortBrand.Numeric:
                Assert.Equal(Convert.ToDouble(value), flowPort.Port.Value);
                break;
            case PortBrand.Rectangle:
                Assert.Equal(JsonSerializer.Deserialize<RectangleDto>(value), flowPort.Port.Value);
                break;
            case PortBrand.Image:
                ImageDto? image = JsonSerializer.Deserialize<ImageDto>(value);
                if(topic.EndsWith("meta"))
                {
                    Assert.Equal(image!.Meta.Width, ((ImageDto)flowPort.Port.Value!).Meta.Width);
                    Assert.Equal(image.Meta.Height, ((ImageDto)flowPort.Port.Value!).Meta.Height);
                    Assert.Equal(image.Meta.PixelFormat, ((ImageDto)flowPort.Port.Value!).Meta.PixelFormat);
                    Assert.Equal(image.Meta.EncoderType, ((ImageDto)flowPort.Port.Value!).Meta.EncoderType);
                }
                if(topic.EndsWith("data"))
                {
                    Assert.Equal(image!.Base64, ((ImageDto)flowPort.Port.Value!).Base64);
                }
                break;
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Test_UpdateAsync(bool hasPort)
    {
        // Arrange
        var step = new StepDto { Id = Guid.NewGuid(), ExecutionTimeMs = 0 };
        var port = new PortDto { Id = Guid.NewGuid(), Name = "test", Brand = PortBrand.String };
        _flowServiceMock.Setup(x => x.GetPortAsync(It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync(hasPort ? port : null!);
        using var flowNode = new FlowNode(_flowServiceMock.Object, _mqttClientProviderStepMock.Object, _stateServiceMock.Object, step);
        using var flowPort = new FlowPort(flowNode, port, step, _flowServiceMock.Object, _mqttClientProviderMock.Object, _stateServiceMock.Object);

        int invoked = 0;
        flowPort.PortChanged += () => { invoked++; };

        // Act
        await flowPort.UpdateAsync();

        // Assert
        Assert.Equal(hasPort ? 1 : 0, invoked);
    }

}
