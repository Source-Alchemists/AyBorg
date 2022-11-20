using System.Text;
using AyBorg.SDK.Communication.MQTT;
using AyBorg.SDK.Data.DTOs;
using AyBorg.Web.Pages.Agent.Editor.Nodes;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Services.AppState;
using AyBorg.Web.Shared.Models;
using Moq;
using MQTTnet;

namespace AyBorg.Web.Tests.Pages.Agent.Editor.Nodes;

public class FlowNodeTests
{
    private readonly Mock<IFlowService> _flowServiceMock;
    private readonly Mock<IStateService> _stateServiceMock;
    private readonly Mock<IMqttClientProvider> _mqttClientProviderMock;
    private readonly MqttSubscription _mqttSubscription;

    public FlowNodeTests()
    {
        _flowServiceMock = new Mock<IFlowService>();
        _stateServiceMock = new Mock<IStateService>();
        _stateServiceMock.Setup(x => x.AgentState).Returns(new UiAgentState { UniqueName = "test" });

        _mqttSubscription = new MqttSubscription { TopicFilter = "test" };

        _mqttClientProviderMock = new Mock<IMqttClientProvider>();
        _mqttClientProviderMock.Setup(x => x.SubscribeAsync(It.IsAny<string>())).ReturnsAsync(_mqttSubscription);
    }


    [Fact]
    public void TestMqttMessageReceived()
    {
        // Arrange
        var step = new StepDto { Id = Guid.NewGuid(), ExecutionTimeMs = 0 };
        using var flowNode = new FlowNode(_flowServiceMock.Object, _mqttClientProviderMock.Object, _stateServiceMock.Object, step);
        bool invoked = false;
        flowNode.StepChanged += () => { invoked = true; };

        // Act
       _mqttSubscription.MessageReceived!.Invoke(new MqttApplicationMessage { Payload = Encoding.UTF8.GetBytes("100") });

        // Assert
        Assert.True(invoked);
        Assert.Equal(100, step.ExecutionTimeMs);
    }

    [Fact]
    public void TestDelete()
    {
        // Arrange
        var step = new StepDto { Id = Guid.NewGuid(), ExecutionTimeMs = 0 };
        using var flowNode = new FlowNode(_flowServiceMock.Object, _mqttClientProviderMock.Object, _stateServiceMock.Object, step);
        bool invoked = false;
        flowNode.OnDelete += () => { invoked = true; };

        // Act
        flowNode.Delete();

        // Assert
        Assert.True(invoked);
    }

    [Fact]
    public void TestAddPorts()
    {
        // Arrange
        var step = new StepDto { Id = Guid.NewGuid(), ExecutionTimeMs = 0, Ports = new List<PortDto> { new PortDto { Id = Guid.NewGuid() } } };
        using var flowNode = new FlowNode(_flowServiceMock.Object, _mqttClientProviderMock.Object, _stateServiceMock.Object, step);

        // Act
        flowNode.AddPort(new FlowPort(flowNode, new PortDto { Id = Guid.NewGuid() }, step, _flowServiceMock.Object, _mqttClientProviderMock.Object, _stateServiceMock.Object));

        // Assert
        Assert.Equal(2, flowNode.Ports.Count);
    }
}
