using AyBorg.Agent.Runtime;
using AyBorg.Agent.Services;
using AyBorg.SDK.Common;
using AyBorg.SDK.Projects;
using AyBorg.SDK.System.Runtime;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Agent.Tests.Services;

public sealed class EngineHostTests : IDisposable
{
    private static readonly NullLogger<EngineHost> s_logger = new();
    private readonly Mock<IEngineFactory> _mockEngineFactory = new();
    private readonly Mock<IEngine> _mockEngine = new();
    private readonly Mock<ICacheService> _mockCacheService = new();
    private readonly Mock<INotifyService> _mockNotifyService = new();
    private readonly CommunicationStateProvider _communicationStateProvider = new();
    private readonly EngineHost _service;
    private bool _isDisposed;

    public EngineHostTests()
    {
        _service = new EngineHost(s_logger,
                                _mockEngineFactory.Object,
                                _mockCacheService.Object,
                                _communicationStateProvider,
                                _mockNotifyService.Object);

        _mockEngineFactory.Setup(e => e.CreateEngine(It.IsAny<Project>(), It.IsAny<EngineExecutionType>())).Returns(_mockEngine.Object);
    }

    [Fact]
    public async ValueTask Test_TryActivateProjectAsync()
    {
        // Arrange
        var project = new Project();

        // Act
        bool result = await _service.TryActivateProjectAsync(project);

        // Assert
        Assert.True(result);
        Assert.Same(project, _service.ActiveProject);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async ValueTask Test_TryDeactivateProjectAsync(bool hasActiveProject, bool hasEngine)
    {
        // Arrange
        if (hasActiveProject)
        {
            var mockStepProxy = new Mock<IStepProxy>();
            mockStepProxy.Setup(x => x.Dispose());
            Project project = new();
            project.Steps.Add(mockStepProxy.Object);
            await _service.TryActivateProjectAsync(project);
        }

        if (hasEngine)
        {
            await _service.StartRunAsync(EngineExecutionType.SingleRun);
        }

        // Act
        bool result = await _service.TryDeactivateProjectAsync();

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async ValueTask Test_GetEngineStatusAsync(bool hasActiveProject, bool hasEngine)
    {
        // Arrange
        if (hasActiveProject)
        {
            var mockStepProxy = new Mock<IStepProxy>();
            mockStepProxy.Setup(x => x.Dispose());
            Project project = new();
            project.Steps.Add(mockStepProxy.Object);
            await _service.TryActivateProjectAsync(project);
        }

        if (hasEngine)
        {
            _mockEngine.Setup(e => e.TryStartAsync()).ReturnsAsync(true);
            _mockEngine.Setup(e => e.Meta).Returns(new EngineMeta
            {
                State = EngineState.Running
            });
            await _service.StartRunAsync(EngineExecutionType.SingleRun);
        }

        // Act
        EngineMeta result = _service.GetEngineStatus();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(hasEngine ? EngineState.Running : EngineState.Idle, result.State);
    }

    [Theory]
    [InlineData(false, false, EngineExecutionType.SingleRun)]
    [InlineData(true, false, EngineExecutionType.SingleRun)]
    [InlineData(false, true, EngineExecutionType.SingleRun)]
    [InlineData(true, true, EngineExecutionType.SingleRun)]
    [InlineData(false, false, EngineExecutionType.ContinuousRun)]
    [InlineData(true, false, EngineExecutionType.ContinuousRun)]
    [InlineData(false, true, EngineExecutionType.ContinuousRun)]
    [InlineData(true, true, EngineExecutionType.ContinuousRun)]
    public async ValueTask Test_StartRunAsync(bool hasActiveProject, bool hasEngine, EngineExecutionType executionType)
    {
        // Arrange
        if (hasActiveProject)
        {
            var mockStepProxy = new Mock<IStepProxy>();
            mockStepProxy.Setup(x => x.Dispose());
            Project project = new();
            project.Steps.Add(mockStepProxy.Object);
            await _service.TryActivateProjectAsync(project);
        }

        if (hasEngine)
        {
            _mockEngine.Setup(e => e.TryStartAsync()).ReturnsAsync(true);
            _mockEngine.Setup(e => e.Meta).Returns(new EngineMeta
            {
                State = EngineState.Running
            });
        }

        // Act
        EngineMeta result = await _service.StartRunAsync(executionType);

        // Assert
        Assert.NotNull(result);
        if (!hasActiveProject || !hasEngine)
        {
            Assert.Equal(EngineState.Idle, result.State);
        }
        else
        {
            Assert.Equal(EngineState.Running, result.State);
        }
    }

    [Theory]
    [InlineData(false, false, EngineState.Idle)]
    [InlineData(true, false, EngineState.Idle)]
    [InlineData(false, true, EngineState.Idle)]
    [InlineData(true, true, EngineState.Idle)]
    [InlineData(false, false, EngineState.Running)]
    [InlineData(true, false, EngineState.Running)]
    [InlineData(false, true, EngineState.Running)]
    [InlineData(true, true, EngineState.Running)]
    public async ValueTask Test_StopRunAsync(bool hasActiveProject, bool hasEngine, EngineState engineState)
    {
        // Arrange
        if (hasActiveProject)
        {
            var mockStepProxy = new Mock<IStepProxy>();
            mockStepProxy.Setup(x => x.Dispose());
            Project project = new();
            project.Steps.Add(mockStepProxy.Object);
            await _service.TryActivateProjectAsync(project);
        }

        if (hasEngine)
        {
            await _service.StartRunAsync(EngineExecutionType.SingleRun);
            _mockEngine.Setup(e => e.TryStopAsync()).ReturnsAsync(true);
            _mockEngine.Setup(e => e.Meta).Returns(new EngineMeta
            {
                State = engineState
            });
        }

        // Act
        EngineMeta result = await _service.StopRunAsync();

        // Assert
        Assert.NotNull(result);
        if (!hasActiveProject || !hasEngine)
        {
            Assert.Equal(EngineState.Idle, result.State);
        }
        else
        {
            Assert.Equal(engineState, result.State);
        }
    }

    [Theory]
    [InlineData(false, false, EngineState.Idle)]
    [InlineData(true, false, EngineState.Idle)]
    [InlineData(false, true, EngineState.Idle)]
    [InlineData(true, true, EngineState.Idle)]
    [InlineData(false, false, EngineState.Aborting)]
    [InlineData(true, false, EngineState.Aborting)]
    [InlineData(false, true, EngineState.Aborting)]
    [InlineData(true, true, EngineState.Aborting)]
    public async ValueTask Test_AbortRunAsync(bool hasActiveProject, bool hasEngine, EngineState engineState)
    {
        // Arrange
        if (hasActiveProject)
        {
            var mockStepProxy = new Mock<IStepProxy>();
            mockStepProxy.Setup(x => x.Dispose());
            Project project = new();
            project.Steps.Add(mockStepProxy.Object);
            await _service.TryActivateProjectAsync(project);
        }

        if (hasEngine)
        {
            await _service.StartRunAsync(EngineExecutionType.SingleRun);
            _mockEngine.Setup(e => e.TryAbortAsync()).ReturnsAsync(true);
            _mockEngine.Setup(e => e.Meta).Returns(new EngineMeta
            {
                State = engineState
            });
        }

        // Act
        EngineMeta result = await _service.AbortRunAsync();

        // Assert
        Assert.NotNull(result);
        if (!hasActiveProject || !hasEngine)
        {
            Assert.Equal(EngineState.Idle, result.State);
        }
        else
        {
            Assert.Equal(engineState, result.State);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_isDisposed && disposing)
        {
            _service.Dispose();
            _isDisposed = true;
        }
    }
}
