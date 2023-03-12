using Ayborg.Gateway.Audit.V1;
using AyBorg.Web.Tests.Helpers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Web.Services.Tests;

public class AuditServiceTests
{
    private static readonly NullLogger<AuditService> s_logger = new();
    private readonly Mock<Audit.AuditClient> _mockAuditClient = new();
    private readonly AuditService _service;

    public AuditServiceTests()
    {
        _service = new AuditService(s_logger, _mockAuditClient.Object);
    }

    [Fact]
    public async Task Test_GetChangesetsAsync()
    {
        // Arrange
        AsyncServerStreamingCall<AuditChangeset> callStream = GrpcCallHelpers.CreateAsyncServerStreamingCall(new List<AuditChangeset> {
            new AuditChangeset {
                Token = Guid.NewGuid().ToString(),
                ProjectId = Guid.NewGuid().ToString(),
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            }
        });
        _mockAuditClient.Setup(m => m.GetChangesets(It.IsAny<GetAuditChangesetsRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(callStream);

        // Act
        await foreach (Shared.Models.AuditChangeset changeset in _service.GetChangesetsAsync())
        {
            // Assert
            Assert.NotNull(changeset);
        }
    }

    [Fact]
    public async Task Test_GetChanges()
    {
        // Arrange
        AsyncServerStreamingCall<AuditChange> callStream = GrpcCallHelpers.CreateAsyncServerStreamingCall(new List<AuditChange> {
            new AuditChange {
                RelatedTokenA = Guid.NewGuid().ToString(),
                RelatedTokenB = Guid.NewGuid().ToString()
            }
        });
        _mockAuditClient.Setup(m => m.GetChanges(It.IsAny<GetAuditChangesRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(callStream);

        // Act
        await foreach (Shared.Models.AuditChange change in _service.GetChangesAsync(new List<Shared.Models.AuditChangeset>()))
        {
            // Assert
            Assert.NotNull(change);
        }
    }

    [Fact]
    public async Task Test_GetReports()
    {
        // Arrange
        AsyncServerStreamingCall<AuditReport> callStream = GrpcCallHelpers.CreateAsyncServerStreamingCall(new List<AuditReport> {
            new AuditReport {
                Id = Guid.NewGuid().ToString(),
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            }
        });
        _mockAuditClient.Setup(m => m.GetReports(It.IsAny<Empty>(), null, null, It.IsAny<CancellationToken>())).Returns(callStream);

        // Act
        await foreach (Shared.Models.AuditReport report in _service.GetReportsAsync())
        {
            // Assert
            Assert.NotNull(report);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Test_TryAddReport(bool canAdd)
    {
        // Arrange
        var changesets = new List<Shared.Models.AuditChangeset> {
            new Shared.Models.AuditChangeset {
                Timestamp = DateTime.UtcNow
            },
            new Shared.Models.AuditChangeset {
                Timestamp = DateTime.UtcNow
            }
        };
        AsyncUnaryCall<Empty> mockCallAddFlowStep = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
        if (canAdd)
        {
            _mockAuditClient.Setup(m => m.AddReportAsync(It.IsAny<AddAuditReportRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(mockCallAddFlowStep);
        }
        else
        {
            _mockAuditClient.Setup(m => m.AddReportAsync(It.IsAny<AddAuditReportRequest>(), null, null, It.IsAny<CancellationToken>())).Throws(() => new RpcException(Status.DefaultCancelled));
        }

        // Act
        bool result = await _service.TryAddReport("Test", "Comment", changesets);

        // Assert
        Assert.Equal(canAdd, result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Test_TryDeleteReport(bool canAdd)
    {
        // Arrange
        var report = new Shared.Models.AuditReport {
            Timestamp = DateTime.UtcNow
        };
        AsyncUnaryCall<Empty> mockCallAddFlowStep = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
        if (canAdd)
        {
            _mockAuditClient.Setup(m => m.DeleteReportAsync(It.IsAny<AuditReport>(), null, null, It.IsAny<CancellationToken>())).Returns(mockCallAddFlowStep);
        }
        else
        {
            _mockAuditClient.Setup(m => m.DeleteReportAsync(It.IsAny<AuditReport>(), null, null, It.IsAny<CancellationToken>())).Throws(() => new RpcException(Status.DefaultCancelled));
        }

        // Act
        bool result = await _service.TryDeleteReport(report);

        // Assert
        Assert.Equal(canAdd, result);
    }
}
