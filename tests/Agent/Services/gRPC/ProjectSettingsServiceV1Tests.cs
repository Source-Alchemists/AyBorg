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
using AyBorg.Agent.Services;
using AyBorg.Agent.Services.gRPC;
using AyBorg.Authorization;
using AyBorg.Data.Agent;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Moq;

namespace AyBorg.Agent.Tests.Services.gRPC;

public class ProjectSettingsServiceV1Tests : BaseGrpcServiceTests<ProjectSettingsServiceV1, ProjectSettings.ProjectSettingsClient>
{
    private readonly Mock<IProjectSettingsService> _mockProjectSettingsService = new();

    public ProjectSettingsServiceV1Tests()
    {
        _service = new ProjectSettingsServiceV1(_mockProjectSettingsService.Object);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Test_GetProjectSettings(bool hasInvalidProjectId)
    {
        // Arrange
        _mockProjectSettingsService.Setup(m => m.GetSettingsRecordAsync(It.IsAny<Guid>())).ReturnsAsync(new ProjectSettingsRecord
        {
            IsForceResultCommunicationEnabled = true
        });
        var request = new GetProjectSettingsRequest
        {
            ProjectDbId = hasInvalidProjectId ? "42" : Guid.NewGuid().ToString()
        };

        // Act
        if (hasInvalidProjectId)
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.GetProjectSettings(request, _serverCallContext));
            return;
        }

        GetProjectSettingsResponse response = await _service.GetProjectSettings(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.ProjectSettings.IsForceResultCommunicationEnabled);
    }

    [Theory]
    [InlineData(Roles.Administrator, true, false, false)]
    [InlineData(Roles.Engineer, false, false, false)]
    [InlineData(Roles.Reviewer, false, false, false)]
    [InlineData(Roles.Auditor, false, false, false)]
    [InlineData(Roles.Administrator, true, true, false)]
    [InlineData(Roles.Administrator, true, false, true)]
    public async Task Test_UpdateProjectSettings(string userRole, bool isAllowed, bool hasInvalidProjectId, bool isUpdateFailing)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockProjectSettingsService.Setup(m => m.TryUpdateActiveProjectSettingsAsync(It.IsAny<Guid>(), It.IsAny<AyBorg.Runtime.Projects.ProjectSettings>())).ReturnsAsync(!isUpdateFailing);
        var request = new UpdateProjectSettingsRequest
        {
            ProjectDbId = hasInvalidProjectId ? "42" : Guid.NewGuid().ToString(),
            ProjectSettings = new ProjectSettingsDto()
        };

        // Act
        if (!isAllowed)
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.UpdateProjectSettings(request, _serverCallContext));
            return;
        }

        if (hasInvalidProjectId || isUpdateFailing)
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.UpdateProjectSettings(request, _serverCallContext));
            return;
        }

        Empty response = await _service.UpdateProjectSettings(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
    }
}
