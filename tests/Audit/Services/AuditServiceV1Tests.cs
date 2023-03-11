using Ayborg.Gateway.Audit.V1;
using AyBorg.Data.Audit.Models;
using AyBorg.Data.Audit.Models.Agent;
using AyBorg.Data.Audit.Repositories;
using AyBorg.Data.Audit.Repositories.Agent;
using AyBorg.SDK.Projects;
using AyBorg.SDK.System;
using Grpc.Core;
using Moq;

namespace AyBorg.Audit.Services.Tests;

public class AuditServiceV1Tests : BaseGrpcServiceTests<AuditServiceV1, Ayborg.Gateway.Audit.V1.Audit.AuditClient>
{
    private readonly Mock<IAgentAuditService> _mockAgentAuditService = new();
    private readonly Mock<IProjectAuditRepository> _mockProjectAuditRepository = new();
    private readonly Mock<IAuditReportRepository> _mockAuditReportRepository = new();

    public AuditServiceV1Tests()
    {
        _service = new AuditServiceV1(s_logger,
                                        _mockAgentAuditService.Object,
                                        _mockProjectAuditRepository.Object,
                                        _mockAuditReportRepository.Object);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async ValueTask Test_AddEntry(bool canAdd)
    {
        // Arrange
        var request = new AuditEntry
        {
            Token = Guid.NewGuid().ToString(),
            ServiceType = "Agent",
            ServiceUniqueName = "Agent",
            User = "User",
            Timestamp = new Google.Protobuf.WellKnownTypes.Timestamp(),
            AgentProject = new AgentProjectAuditEntry
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Project",
                State = (int)ProjectState.Draft,
                VersionName = "Version",
                VersionIteration = 1,
                Comment = "Comment",
                ApprovedBy = "User",
                Settings = new AgentProjectSettingsAuditEntry()
            }
        };

        _mockAgentAuditService.Setup(m => m.TryAdd(It.IsAny<ProjectAuditRecord>())).Returns(canAdd);

        // Act
        if (canAdd)
        {
            Google.Protobuf.WellKnownTypes.Empty response = await _service.AddEntry(request, _serverCallContext);

            // Assert
            Assert.NotNull(response);
        }
        else
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.AddEntry(request, _serverCallContext));
        }
    }

    [Theory]
    [InlineData(true, ServiceTypes.Agent)]
    [InlineData(false, ServiceTypes.Agent)]
    [InlineData(false, ServiceTypes.Analytics)]
    public async ValueTask Test_InvalidateEntry(bool canRemove, string serviceType)
    {
        // Arrange
        var request = new InvalidateAuditEntryRequest
        {
            Token = Guid.NewGuid().ToString(),
            ServiceType = serviceType
        };

        _mockAgentAuditService.Setup(m => m.TryRemove(It.IsAny<Guid>())).Returns(canRemove);

        // Act
        if (canRemove && serviceType.Equals(ServiceTypes.Agent))
        {
            Google.Protobuf.WellKnownTypes.Empty response = await _service.InvalidateEntry(request, _serverCallContext);

            // Assert
            Assert.NotNull(response);
        }
        else
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.InvalidateEntry(request, _serverCallContext));
        }
    }

    [Fact]
    public async ValueTask Test_GetChangesets()
    {
        // Arrange
        _mockAgentAuditService.Setup(m => m.GetChangesets()).Returns(new List<ChangesetRecord> {
            new ChangesetRecord()
        });
        var mockServerStreamWriter = new Mock<IServerStreamWriter<AuditChangeset>>();

        // Act
        await _service.GetChangesets(new GetAuditChangesetsRequest(), mockServerStreamWriter.Object, _serverCallContext);

        // Assert
        mockServerStreamWriter.Verify(m => m.WriteAsync(It.IsAny<AuditChangeset>()), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async ValueTask Test_GetChanges(bool containsChangesets)
    {
        // Arrange
        var changesetAId = Guid.NewGuid();
        var changesetBId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        if (containsChangesets)
        {
            _mockProjectAuditRepository.Setup(m => m.FindAll()).Returns(new List<ProjectAuditRecord> {
            new ProjectAuditRecord {
                Id = changesetAId,
                ProjectId = projectId,
                Settings = new ProjectSettingsAuditRecord {
                    IsForceResultCommunicationEnabled = false
                }
            },
            new ProjectAuditRecord {
                Id = changesetBId,
                ProjectId = projectId,
                Settings = new ProjectSettingsAuditRecord {
                    IsForceResultCommunicationEnabled = true
                }
            }
        });
        }
        var request = new GetAuditChangesRequest();
        request.Tokens.Add(changesetAId.ToString());
        request.Tokens.Add(changesetBId.ToString());
        var mockServerStreamWriter = new Mock<IServerStreamWriter<AuditChange>>();

        // Act
        if (containsChangesets)
        {
            await _service.GetChanges(request, mockServerStreamWriter.Object, _serverCallContext);

            // Assert
            mockServerStreamWriter.Verify(m => m.WriteAsync(It.IsAny<AuditChange>()), Times.Once);
        }
        else
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.GetChanges(request, mockServerStreamWriter.Object, _serverCallContext));
        }
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public async ValueTask Test_AddReport(bool containsRequestedChangesets, bool canAdd)
    {
        // Arrange
        var changesetId = Guid.NewGuid();
        var request = new AddAuditReportRequest
        {
            ReportName = "Report",
            Comment = "Comment",
        };

        request.Changesets.Add(new AuditChangeset
        {
            Token = changesetId.ToString()
        });

        if (containsRequestedChangesets)
        {
            _mockProjectAuditRepository.Setup(m => m.FindAll()).Returns(new List<ProjectAuditRecord> {
                new ProjectAuditRecord {
                    Id = changesetId
                }
            });
        }

        _mockAuditReportRepository.Setup(m => m.TryAdd(It.IsAny<AuditReportRecord>())).Returns(canAdd);

        if (containsRequestedChangesets && canAdd)
        {
            // Act
            Google.Protobuf.WellKnownTypes.Empty response = await _service.AddReport(request, _serverCallContext);

            // Assert
            Assert.NotNull(response);
        }
        else
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.AddReport(request, _serverCallContext));
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async ValueTask Test_DeleteReport(bool canRemove)
    {
        // Arrang
        var request = new AuditReport
        {
            Id = Guid.NewGuid().ToString(),
            Timestamp = new Google.Protobuf.WellKnownTypes.Timestamp()
        };

        _mockAuditReportRepository.Setup(m => m.TryRemove(It.IsAny<AuditReportRecord>())).Returns(canRemove);


        if (canRemove)
        {
            // Act
            Google.Protobuf.WellKnownTypes.Empty response = await _service.DeleteReport(request, _serverCallContext);

            // Assert
            Assert.NotNull(response);
        }
        else
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.DeleteReport(request, _serverCallContext));
        }
    }

    [Fact]
    public async ValueTask Test_GetReports()
    {
        // Arrange
        var mockServerStreamWriter = new Mock<IServerStreamWriter<AuditReport>>();
        _mockAuditReportRepository.Setup(m => m.FindAll()).Returns(new List<AuditReportRecord> {
            new AuditReportRecord()
        });

        // Act
        await _service.GetReports(new Google.Protobuf.WellKnownTypes.Empty(), mockServerStreamWriter.Object, _serverCallContext);

        // Assert
        mockServerStreamWriter.Verify(m => m.WriteAsync(It.IsAny<AuditReport>()), Times.Once);
    }
}
