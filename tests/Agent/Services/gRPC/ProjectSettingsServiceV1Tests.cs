using System.Security.Claims;
using Ayborg.Gateway.Agent.V1;
using AyBorg.Agent.Services;
using AyBorg.Agent.Services.gRPC;
using AyBorg.Data.Agent;
using AyBorg.SDK.Authorization;
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
        _mockProjectSettingsService.Setup(m => m.TryUpdateActiveProjectSettingsAsync(It.IsAny<Guid>(), It.IsAny<SDK.Projects.ProjectSettings>())).ReturnsAsync(!isUpdateFailing);
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
