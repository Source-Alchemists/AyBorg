using AyBorg.Agent.Services;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Projects;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Agent.Tests.Services;

public class FlowServiceTests
{
    private readonly NullLogger<FlowService> _logger = new();
    private readonly Mock<IPluginsService> _mockPluginsService = new();
    private readonly Mock<IEngineHost> _mockEngineHost = new();
    private readonly Mock<IRuntimeConverterService> _mockRuntimeConverterService = new();
    private readonly Mock<INotifyService> _mockNotifyService = new();
    private readonly FlowService _service;

    public FlowServiceTests()
    {
        _service = new FlowService(_logger,
                                        _mockPluginsService.Object,
                                        _mockEngineHost.Object,
                                        _mockRuntimeConverterService.Object,
                                        _mockNotifyService.Object);
    }

    [Fact]
    public async Task Test_LinkPorts()
    {
        // Arrange
        IStepProxy step1 = CreateStepWithPort("Step1", PortDirection.Output).Object;
        IStepProxy step2 = CreateStepWithPort("Step2", PortDirection.Input).Object;

        _mockEngineHost.Setup(x => x.ActiveProject).Returns(new Project
        {
            Steps = new List<IStepProxy> { step1, step2 }
        });

        // Act
        PortLink result = await _service.LinkPortsAsync(step1.Ports.First().Id, step2.Ports.First().Id);

        // Assert
        Assert.Equal(step1.Ports.First().Id, result.SourceId);
        Assert.Equal(step2.Ports.First().Id, result.TargetId);
    }

    [Fact]
    public async Task Test_LinkPorts_WrongDirection()
    {
        // Arrange
        IStepProxy step1 = CreateStepWithPort("Step1", PortDirection.Input).Object;
        IStepProxy step2 = CreateStepWithPort("Step2", PortDirection.Output).Object;

        _mockEngineHost.Setup(x => x.ActiveProject).Returns(new Project
        {
            Steps = new List<IStepProxy> { step1, step2 }
        });

        // Act
        PortLink result = await _service.LinkPortsAsync(step1.Ports.First().Id, step2.Ports.First().Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Test_UnlinkPorts()
    {
        // Arrange
        IStepProxy step1 = CreateStepWithPort("Step1", PortDirection.Output).Object;
        IStepProxy step2 = CreateStepWithPort("Step2", PortDirection.Input).Object;

        var link = new PortLink(step1.Ports.First(), step2.Ports.First());
        step1.Ports.First().Connect(link);
        step2.Ports.First().Connect(link);
        step1.Links.Add(link);
        step2.Links.Add(link);

        _mockEngineHost.Setup(x => x.ActiveProject).Returns(new Project
        {
            Steps = new List<IStepProxy> { step1, step2 }
        });

        // Act
        bool result = await _service.TryUnlinkPortsAsync(link.Id);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Test_GetSteps(bool activeProject)
    {
        // Arrange
        IStepProxy step1 = CreateStepWithPort("Step1", PortDirection.Output).Object;
        IStepProxy step2 = CreateStepWithPort("Step2", PortDirection.Input).Object;
        Project project = new()
        {
            Steps = new List<IStepProxy> { step1, step2 }
        };
        _mockEngineHost.SetupGet(x => x.ActiveProject).Returns(activeProject ? project : null);

        // Act
        IEnumerable<IStepProxy> result = _service.GetSteps();

        // Assert
        Assert.Equal(activeProject ? 2 : 0, result.Count());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Test_GetLinks(bool activeProject)
    {
        // Arrange
        Project project = new()
        {
            Links = new List<PortLink> { new PortLink(new Mock<IPort>().Object, new Mock<IPort>().Object) }
        };
        _mockEngineHost.SetupGet(x => x.ActiveProject).Returns(activeProject ? project : null);

        // Act
        IEnumerable<PortLink> result = _service.GetLinks();

        // Assert
        Assert.Equal(activeProject ? 1 : 0, result.Count());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Test_GetPort(bool activeProject, bool portAvailable)
    {
        // Arrange
        Mock<IStepProxy> mockStep = CreateStepWithPort("Step1", PortDirection.Output);
        Project project = new()
        {
            Steps = new List<IStepProxy> { mockStep.Object }
        };
        _mockEngineHost.SetupGet(x => x.ActiveProject).Returns(activeProject ? project : null);

        // Act
        IPort? result = _service.GetPort(portAvailable ? mockStep.Object.Ports.First().Id : Guid.NewGuid());

        // Assert
        Assert.Equal(portAvailable, result != null);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Test_AddStepAsync(bool activeProject, bool pluginAvailable)
    {
        // Arrange
        Mock<IStepProxy> mockStep = CreateStepWithPort("Step", PortDirection.Output);
        if(pluginAvailable)
        {
            _mockPluginsService.Setup(x => x.Find(It.IsAny<Guid>())).Returns(new Mock<IStepProxy>().Object);
            _mockPluginsService.Setup(x => x.CreateInstance(It.IsAny<IStepBody>())).Returns(mockStep.Object);
        }

        Project project = new();
        _mockEngineHost.SetupGet(x => x.ActiveProject).Returns(activeProject ? project : null);

        // Act
        IStepProxy result = await _service.AddStepAsync(Guid.NewGuid(), 123, 456);

        // Assert
        if (activeProject && pluginAvailable)
        {
            Assert.NotNull(result);
            Assert.Single(project.Steps);
            mockStep.VerifySet(x => x.X = 123);
            mockStep.VerifySet(x => x.Y = 456);
            _mockPluginsService.Verify(x => x.CreateInstance(It.IsAny<IStepBody>()), Times.Once);
        }
        else
        {
            Assert.Null(result);
            Assert.Empty(project.Steps);
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Test_TryRemoveStepAsync(bool activeProject, bool stepAvailable)
    {
        // Arrange
        IStepProxy step1 = CreateStepWithPort("Step1", PortDirection.Output).Object;
        IStepProxy step2 = CreateStepWithPort("Step2", PortDirection.Input).Object;

        var link = new PortLink(step1.Ports.First(), step2.Ports.First());
        step1.Ports.First().Connect(link);
        step2.Ports.First().Connect(link);
        step1.Links.Add(link);
        step2.Links.Add(link);
        Project project = new()
        {
            Steps = new List<IStepProxy> { step1, step2 },
            Links = new List<PortLink> { link }
        };
        _mockEngineHost.SetupGet(x => x.ActiveProject).Returns(activeProject ? project : null);

        // Act
        bool result = await _service.TryRemoveStepAsync(stepAvailable ? step1.Id : Guid.NewGuid());

        // Assert
        if (activeProject && stepAvailable)
        {
            Assert.True(result);
            Assert.Single(project.Steps);
            Assert.Empty(project.Links);
        }
        else
        {
            Assert.False(result);
            Assert.Equal(2, project.Steps.Count);
            Assert.Single(project.Links);
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Test_TryMoveStepAsync(bool activeProject, bool stepAvailable)
    {
        // Arrange
        Mock<IStepProxy> mockStep = CreateStepWithPort("Step", PortDirection.Output);
        Project project = new()
        {
            Steps = new List<IStepProxy> { mockStep.Object }
        };
        _mockEngineHost.SetupGet(x => x.ActiveProject).Returns(activeProject ? project : null);

        // Act
        bool result = await _service.TryMoveStepAsync(stepAvailable ? mockStep.Object.Id : Guid.NewGuid(), 123, 456);

        // Assert
        if (activeProject && stepAvailable)
        {
            Assert.True(result);
            mockStep.VerifySet(x => x.X = 123);
            mockStep.VerifySet(x => x.Y = 456);
        }
        else
        {
            Assert.False(result);
            mockStep.VerifySet(x => x.X = 123, Times.Never);
            mockStep.VerifySet(x => x.Y = 456, Times.Never);
        }
    }

    private static Mock<IStepProxy> CreateStepWithPort(string stepName, PortDirection direction)
    {
        var stepMock = new Mock<IStepProxy>();
        var portMock = new Mock<IPort>();
        portMock.Setup(x => x.Id).Returns(Guid.NewGuid());
        portMock.Setup(x => x.Direction).Returns(direction);

        stepMock.Setup(x => x.Id).Returns(Guid.NewGuid());
        stepMock.Setup(x => x.Name).Returns(stepName);
        stepMock.Setup(x => x.Links).Returns(new List<PortLink>());
        stepMock.Setup(x => x.Ports).Returns(new[] { portMock.Object });
        stepMock.SetupSet(x => x.X = It.IsAny<int>()).Verifiable();
        stepMock.SetupSet(x => x.Y = It.IsAny<int>()).Verifiable();
        return stepMock;
    }
}
