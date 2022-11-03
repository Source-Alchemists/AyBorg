using Atomy.Agent.Hubs;
using Atomy.Agent.Services;
using Atomy.SDK.Common;
using Atomy.SDK.Common.Ports;
using Atomy.SDK.Projects;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Atomy.Agent.Tests.Services;

public class FlowServiceTests
{
    private readonly NullLogger<FlowService> _logger = new NullLogger<FlowService>();

    [Fact]
    public async Task TestLinkPorts()
    {
        // Arrange
        var pluginServiceMock = new Mock<IPluginsService>();
        var runtimeHostMock = new Mock<IEngineHost>();
        var flowHubMock = new Mock<IFlowHub>();
        var runtimeConverterServiceMock = new Mock<IRuntimeConverterService>();

        var step1 = CreateStepWithPort("Step1", PortDirection.Output);
        var step2 = CreateStepWithPort("Step2", PortDirection.Input);

        runtimeHostMock.Setup(x => x.ActiveProject).Returns(new Project
        {
            Steps = new List<IStepProxy> { step1, step2 }
        });

        var service = new FlowService(_logger, 
                                        pluginServiceMock.Object, 
                                        runtimeHostMock.Object, 
                                        flowHubMock.Object, 
                                        runtimeConverterServiceMock.Object);

        // Act
        var result = await service.LinkPortsAsync(step1.Ports.First().Id, step2.Ports.First().Id);

        // Assert
        Assert.Equal(step1.Ports.First().Id, result.SourceId);
        Assert.Equal(step2.Ports.First().Id, result.TargetId);
    }

    [Fact]
    public async Task TestLinkPorts_WrongDirection()
    {
        // Arrange
        var pluginServiceMock = new Mock<IPluginsService>();
        var runtimeHostMock = new Mock<IEngineHost>();
        var flowHubMock = new Mock<IFlowHub>();
        var runtimeConverterServiceMock = new Mock<IRuntimeConverterService>();

        var step1 = CreateStepWithPort("Step1", PortDirection.Input);
        var step2 = CreateStepWithPort("Step2", PortDirection.Output);

        runtimeHostMock.Setup(x => x.ActiveProject).Returns(new Project
        {
            Steps = new List<IStepProxy> { step1, step2 }
        });

        var service = new FlowService(_logger, 
                                        pluginServiceMock.Object, 
                                        runtimeHostMock.Object, 
                                        flowHubMock.Object, 
                                        runtimeConverterServiceMock.Object);

        // Act
        var result = await service.LinkPortsAsync(step1.Ports.First().Id, step2.Ports.First().Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task TestUnlinkPorts()
    {
        // Arrange
        var pluginServiceMock = new Mock<IPluginsService>();
        var runtimeHostMock = new Mock<IEngineHost>();
        var flowHubMock = new Mock<IFlowHub>();
        var runtimeConverterServiceMock = new Mock<IRuntimeConverterService>();

        var step1 = CreateStepWithPort("Step1", PortDirection.Output);
        var step2 = CreateStepWithPort("Step2", PortDirection.Input);

        var link = new PortLink(step1.Ports.First(), step2.Ports.First());
        step1.Ports.First().Connect(link);
        step2.Ports.First().Connect(link);
        step1.Links.Add(link);
        step2.Links.Add(link);

        runtimeHostMock.Setup(x => x.ActiveProject).Returns(new Project
        {
            Steps = new List<IStepProxy> { step1, step2 }
        });

        var service = new FlowService(_logger, 
                                        pluginServiceMock.Object, 
                                        runtimeHostMock.Object, 
                                        flowHubMock.Object, 
                                        runtimeConverterServiceMock.Object);

        // Act
        var result = await service.TryUnlinkPortsAsync(link.Id);

        // Assert
        Assert.True(result);
    }

    private static IStepProxy CreateStepWithPort(string stepName, PortDirection direction)
    {
        var stepMock = new Mock<IStepProxy>();
        var portMock = new Mock<IPort>();
        portMock.Setup(x => x.Id).Returns(Guid.NewGuid());
        portMock.Setup(x => x.Direction).Returns(direction);

        stepMock.Setup(x => x.Id).Returns(Guid.NewGuid());
        stepMock.Setup(x => x.Name).Returns(stepName);
        stepMock.Setup(x => x.Links).Returns(new List<PortLink>());
        stepMock.Setup(x => x.Ports).Returns(new[] { portMock.Object });
        return stepMock.Object;
    }
}