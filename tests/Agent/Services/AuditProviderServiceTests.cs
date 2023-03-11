using Ayborg.Gateway.Audit.V1;
using AyBorg.Agent.Tests.Helpers;
using AyBorg.Data.Agent;
using AyBorg.SDK.System.Configuration;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Agent.Services.Tests;

public class AuditProviderServiceTests
{
    private static readonly NullLogger<AuditProviderService> s_logger = new();
    private readonly Mock<Audit.AuditClient> _mockAuditClient = new();
    private readonly Mock<IServiceConfiguration> _mockServiceConfiguration = new();
    private readonly AuditProviderService _service;

    public AuditProviderServiceTests()
    {
        _mockServiceConfiguration.Setup(m => m.UniqueName).Returns("Agent");
        _service = new AuditProviderService(s_logger, _mockAuditClient.Object, _mockServiceConfiguration.Object);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async ValueTask Test_AddAsync(bool canAdd)
    {
        // Arrange
        var project = new ProjectRecord {
            Meta = new ProjectMetaRecord {
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            }
        };

        if (canAdd)
        {
            AsyncUnaryCall<Empty> mockCallAddFlowStep = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
            _mockAuditClient.Setup(m => m.AddEntryAsync(It.IsAny<AuditEntry>(), null, null, It.IsAny<CancellationToken>())).Returns(mockCallAddFlowStep);
        }
        else
        {
            _mockAuditClient.Setup(m => m.AddEntryAsync(It.IsAny<AuditEntry>(), null, null, It.IsAny<CancellationToken>())).Throws(() => new RpcException(Status.DefaultCancelled));
        }

        // Act
        Guid result = await _service.AddAsync(project, "User");

        // Assert
        if (canAdd)
        {
            Assert.NotEqual(Guid.Empty, result);
        }
        else
        {
            Assert.Equal(Guid.Empty, result);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async ValueTask Test_TryInvalidateAsync(bool canInvalidate)
    {
        // Arrange
        if (canInvalidate)
        {
            AsyncUnaryCall<Empty> mockCallAddFlowStep = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
            _mockAuditClient.Setup(m => m.InvalidateEntryAsync(It.IsAny<InvalidateAuditEntryRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(mockCallAddFlowStep);
        }
        else
        {
            _mockAuditClient.Setup(m => m.InvalidateEntryAsync(It.IsAny<InvalidateAuditEntryRequest>(), null, null, It.IsAny<CancellationToken>())).Throws(() => new RpcException(Status.DefaultCancelled));
        }

        // Act
        bool result = await _service.TryInvalidateAsync(Guid.NewGuid());

        // Assert
        Assert.Equal(canInvalidate, result);
    }
}
