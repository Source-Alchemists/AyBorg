using AyBorg.Agent.Services;
using AyBorg.SDK.Common;
using AyBorg.SDK.Communication.MQTT;
using AyBorg.SDK.Projects;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Agent.Tests.Services;

public sealed class EngineHostTests : IDisposable
{
    private readonly NullLogger<EngineHost> _logger = new();
    private readonly Mock<IEngineFactory> _mockEngineFactory = new();
    private readonly Mock<IMqttClientProvider> _mockMqttClientProvider = new();
    private readonly Mock<ICacheService> _mockCacheService = new();
    private readonly CommunicationStateProvider _communicationStateProvider = new();
    private readonly EngineHost _service;
    private bool _isDisposed;

    public EngineHostTests()
    {
        _service = new EngineHost(_logger,
                                _mockEngineFactory.Object,
                                _mockMqttClientProvider.Object,
                                _mockCacheService.Object,
                                _communicationStateProvider);
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
    [InlineData(false)]
    [InlineData(true)]
    public async ValueTask Test_TryDeactivateProjectAsync(bool activeProjectAvailable)
    {
        // Arrange
        var mockStepProxy = new Mock<IStepProxy>();
        mockStepProxy.Setup(x => x.Dispose());
        Project project = new();
        project.Steps.Add(mockStepProxy.Object);
        if (activeProjectAvailable)
        {
            await _service.TryActivateProjectAsync(project);
        }

        // Act
        bool result = await _service.TryDeactivateProjectAsync();

        // Assert
        Assert.True(result);
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
