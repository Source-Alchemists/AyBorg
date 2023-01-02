using Ayborg.Gateway.Agent.V1;
using AyBorg.Agent.Services;
using AyBorg.SDK.System.Configuration;
using AyBorg.SDK.System.Runtime;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Agent.Tests.Services;

public class NotifyServiceTests
{
    private static readonly NullLogger<NotifyService> s_logger = new();
    private readonly Mock<IServiceConfiguration> _mockServiceConfiguration = new();
    private readonly Mock<Notify.NotifyClient> _mockClient = new();
    private readonly NotifyService _service;

    public NotifyServiceTests()
    {
        _service = new NotifyService(s_logger, _mockServiceConfiguration.Object, _mockClient.Object);
    }

    [Fact]
    public async ValueTask Test_SendEngineStateAsync()
    {
        // Arrange
        EngineMeta engineMeta = CreateEngineMeta();

        // Act
        await _service.SendEngineStateAsync(engineMeta);

        // Assert
        _mockClient.Verify(c => c.CreateNotificationFromAgentAsync(It.IsAny<NotifyMessage>(), null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async ValueTask Test_SendIterationFinishedAsync()
    {
        // Act
        await _service.SendIterationFinishedAsync(Guid.NewGuid());

        // Assert
        _mockClient.Verify(c => c.CreateNotificationFromAgentAsync(It.IsAny<NotifyMessage>(), null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async ValueTask Test_SendAutomationFlowChangedAsync()
    {
        // Act
        await _service.SendAutomationFlowChangedAsync(new SDK.Communication.gRPC.Models.AgentAutomationFlowChangeArgs());

        // Assert
        _mockClient.Verify(c => c.CreateNotificationFromAgentAsync(It.IsAny<NotifyMessage>(), null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    public static EngineMeta CreateEngineMeta()
    {
        return new EngineMeta {
            Id = Guid.NewGuid(),
            State = EngineState.Running,
            ExecutionType = EngineExecutionType.SingleRun
        };
    }
}
