/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Security.Claims;

using Ayborg.Gateway.Audit.V1;
using AyBorg.Authorization;
using AyBorg.Communication;
using AyBorg.Gateway.Models;
using AyBorg.Gateway.Services.Tests;
using AyBorg.Gateway.Tests.Helpers;
using AyBorg.Runtime.Projects;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

using Moq;

namespace AyBorg.Gateway.Services.Audit.Tests;

public class AuditPassthroughServiceV1Tests : BaseGrpcServiceTests<AuditPassthroughServiceV1, Ayborg.Gateway.Audit.V1.Audit.AuditClient>
{
    private readonly Mock<IGatewayConfiguration> _mockConfiguration = new();
    public AuditPassthroughServiceV1Tests()
    {
        _mockGrpcChannelService.Setup(m => m.GetChannelsByTypeName(It.IsAny<string>())).Returns(new List<ChannelInfo> {
            new ChannelInfo()
        });
        _service = new AuditPassthroughServiceV1(_mockGrpcChannelService.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task Test_AddEntry()
    {
        // Arrange
        AsyncUnaryCall<Empty> mockCallAddFlowStep = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
        _mockClient.Setup(c => c.AddEntryAsync(It.IsAny<AuditEntry>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallAddFlowStep);
        var request = new AuditEntry
        {
            Token = Guid.NewGuid().ToString(),
            ServiceType = "TestService",
            ServiceUniqueName = "TestService",
            User = "TestUser",
            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
            AgentProject = new AgentProjectAuditEntry
            {
                Id = Guid.NewGuid().ToString(),
                Name = "TestProject",
                State = (int)ProjectState.Draft,
                VersionName = "TestVersion",
                VersionIteration = 1,
                Comment = "TestComment",
                ApprovedBy = "TestUser",
                Settings = new AgentProjectSettingsAuditEntry
                {
                    IsForceResultCommunicationEnabled = true
                }
            }
        };

        // Act
        Empty response = await _service.AddEntry(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
    }

    [Fact]
    public async Task Test_InvalidateEntry()
    {
        // Arrange
        AsyncUnaryCall<Empty> mockCallAddFlowStep = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
        _mockClient.Setup(c => c.InvalidateEntryAsync(It.IsAny<InvalidateAuditEntryRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallAddFlowStep);
        var request = new InvalidateAuditEntryRequest();

        // Act
        Empty response = await _service.InvalidateEntry(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Auditor, true)]
    [InlineData(Roles.Engineer, false)]
    [InlineData(Roles.Reviewer, false)]
    public async Task Test_GetChangesets(string userRole, bool isAllowed)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        AsyncServerStreamingCall<AuditChangeset> callStream = GrpcCallHelpers.CreateAsyncServerStreamingCall(new List<AuditChangeset> {
            new AuditChangeset()
        });
        _mockClient.Setup(m => m.GetChangesets(It.IsAny<GetAuditChangesetsRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(callStream);
        var mockServerStreamWriter = new Mock<IServerStreamWriter<AuditChangeset>>();

        // Act
        if (isAllowed)
        {
            await _service.GetChangesets(new GetAuditChangesetsRequest(), mockServerStreamWriter.Object, _serverCallContext);

            // Assert
            mockServerStreamWriter.Verify(m => m.WriteAsync(It.IsAny<AuditChangeset>(), It.IsAny<CancellationToken>()));
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetChangesets(new GetAuditChangesetsRequest(), mockServerStreamWriter.Object, _serverCallContext));
        }
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Auditor, true)]
    [InlineData(Roles.Engineer, false)]
    [InlineData(Roles.Reviewer, false)]
    public async Task Test_GetChanges(string userRole, bool isAllowed)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        AsyncServerStreamingCall<AuditChange> callStream = GrpcCallHelpers.CreateAsyncServerStreamingCall(new List<AuditChange> {
            new AuditChange()
        });
        _mockClient.Setup(m => m.GetChanges(It.IsAny<GetAuditChangesRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(callStream);
        var mockServerStreamWriter = new Mock<IServerStreamWriter<AuditChange>>();

        // Act
        if (isAllowed)
        {
            await _service.GetChanges(new GetAuditChangesRequest(), mockServerStreamWriter.Object, _serverCallContext);

            // Assert
            mockServerStreamWriter.Verify(m => m.WriteAsync(It.IsAny<AuditChange>(), It.IsAny<CancellationToken>()));
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetChanges(new GetAuditChangesRequest(), mockServerStreamWriter.Object, _serverCallContext));
        }
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Auditor, true)]
    [InlineData(Roles.Engineer, false)]
    [InlineData(Roles.Reviewer, false)]
    public async Task Test_GetReports(string userRole, bool isAllowed)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        AsyncServerStreamingCall<AuditReport> callStream = GrpcCallHelpers.CreateAsyncServerStreamingCall(new List<AuditReport> {
            new AuditReport()
        });
        _mockClient.Setup(m => m.GetReports(It.IsAny<Empty>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(callStream);
        var mockServerStreamWriter = new Mock<IServerStreamWriter<AuditReport>>();

        // Act
        if (isAllowed)
        {
            await _service.GetReports(new Empty(), mockServerStreamWriter.Object, _serverCallContext);

            // Assert
            mockServerStreamWriter.Verify(m => m.WriteAsync(It.IsAny<AuditReport>(), It.IsAny<CancellationToken>()));
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetReports(new Empty(), mockServerStreamWriter.Object, _serverCallContext));
        }
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Auditor, true)]
    [InlineData(Roles.Engineer, false)]
    [InlineData(Roles.Reviewer, false)]
    public async Task Test_AddReport(string userRole, bool isAllowed)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        AsyncUnaryCall<Empty> mockCallAddFlowStep = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
        _mockClient.Setup(c => c.AddReportAsync(It.IsAny<AddAuditReportRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallAddFlowStep);

        // Act
        if (isAllowed)
        {
            Empty response = await _service.AddReport(new AddAuditReportRequest(), _serverCallContext);

            // Assert
            Assert.NotNull(response);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.AddReport(new AddAuditReportRequest(), _serverCallContext));
        }
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Auditor, false)]
    [InlineData(Roles.Engineer, false)]
    [InlineData(Roles.Reviewer, false)]
    public async Task Test_DeleteReport(string userRole, bool isAllowed)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        AsyncUnaryCall<Empty> mockCallAddFlowStep = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
        _mockClient.Setup(c => c.DeleteReportAsync(It.IsAny<AuditReport>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallAddFlowStep);

        // Act
        if (isAllowed)
        {
            Empty response = await _service.DeleteReport(new AuditReport(), _serverCallContext);

            // Assert
            Assert.NotNull(response);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.DeleteReport(new AuditReport(), _serverCallContext));
        }
    }
}
