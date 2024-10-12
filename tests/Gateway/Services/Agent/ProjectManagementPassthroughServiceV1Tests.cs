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
using Ayborg.Gateway.Agent.V1;
using AyBorg.Authorization;
using AyBorg.Gateway.Services.Tests;
using AyBorg.Gateway.Tests.Helpers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Moq;

namespace AyBorg.Gateway.Services.Agent.Tests;

public class ProjectManagementPassthroughServiceV1Tests : BaseGrpcServiceTests<ProjectManagementPassthroughServiceV1, ProjectManagement.ProjectManagementClient>
{
    public ProjectManagementPassthroughServiceV1Tests()
    {
        _service = new ProjectManagementPassthroughServiceV1(_mockGrpcChannelService.Object);
    }

    [Fact]
    public async Task Test_GetProjectMetas()
    {
        // Arrange
        AsyncUnaryCall<GetProjectMetasResponse> mockCallGetAvailableSteps = GrpcCallHelpers.CreateAsyncUnaryCall(new GetProjectMetasResponse());
        _mockClient.Setup(c => c.GetProjectMetasAsync(It.IsAny<GetProjectMetasRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(mockCallGetAvailableSteps);
        var request = new GetProjectMetasRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        GetProjectMetasResponse resultResponse = await _service.GetProjectMetas(request, _serverCallContext);

        // Assert
        Assert.NotNull(resultResponse);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, true)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_ActivateProject(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<Empty> mockCallActivateProject = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockClient.Setup(c => c.ActivateProjectAsync(It.IsAny<ActivateProjectRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallActivateProject);
        var request = new ActivateProjectRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        Empty resultResponse = null!;
        if (isAllowed)
        {
            resultResponse = await _service.ActivateProject(request, _serverCallContext);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.ActivateProject(request, _serverCallContext));
            return;
        }

        // Assert
        Assert.NotNull(resultResponse);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, false)]
    [InlineData(Roles.Reviewer, true)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_ApproveProject(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<Empty> mockCallApproveProject = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockClient.Setup(c => c.ApproveProjectAsync(It.IsAny<ApproveProjectRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallApproveProject);
        var request = new ApproveProjectRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        Empty resultResponse = null!;
        if (isAllowed)
        {
            resultResponse = await _service.ApproveProject(request, _serverCallContext);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.ApproveProject(request, _serverCallContext));
            return;
        }

        // Assert
        Assert.NotNull(resultResponse);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, false)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_CreateProject(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<CreateProjectResponse> mockCallCreateProject = GrpcCallHelpers.CreateAsyncUnaryCall(new CreateProjectResponse());
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockClient.Setup(c => c.CreateProjectAsync(It.IsAny<CreateProjectRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallCreateProject);
        var request = new CreateProjectRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        CreateProjectResponse resultResponse = null!;
        if (isAllowed)
        {
            resultResponse = await _service.CreateProject(request, _serverCallContext);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.CreateProject(request, _serverCallContext));
            return;
        }

        // Assert
        Assert.NotNull(resultResponse);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, false)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_DeleteProject(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<Empty> mockCallDeleteProject = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockClient.Setup(c => c.DeleteProjectAsync(It.IsAny<DeleteProjectRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallDeleteProject);
        var request = new DeleteProjectRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        Empty resultResponse = null!;
        if (isAllowed)
        {
            resultResponse = await _service.DeleteProject(request, _serverCallContext);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.DeleteProject(request, _serverCallContext));
            return;
        }

        // Assert
        Assert.NotNull(resultResponse);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, true)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_SaveProject(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<Empty> mockCallSaveProject = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockClient.Setup(c => c.SaveProjectAsync(It.IsAny<SaveProjectRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallSaveProject);
        var request = new SaveProjectRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        Empty resultResponse = null!;
        if (isAllowed)
        {
            resultResponse = await _service.SaveProject(request, _serverCallContext);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.SaveProject(request, _serverCallContext));
            return;
        }

        // Assert
        Assert.NotNull(resultResponse);
    }
}
