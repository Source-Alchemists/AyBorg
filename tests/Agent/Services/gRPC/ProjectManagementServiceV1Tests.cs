using System.Security.Claims;
using Ayborg.Gateway.Agent.V1;
using AyBorg.Agent.Services;
using AyBorg.Agent.Services.gRPC;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.Data.DAL;
using AyBorg.SDK.Projects;
using Grpc.Core;
using Moq;

namespace AyBorg.Agent.Tests.Services.gRPC;

public class ProjectManagementServiceV1Tests : BaseGrpcServiceTests<ProjectManagementServiceV1, ProjectManagement.ProjectManagementClient>
{
    private readonly Mock<IProjectManagementService> _mockProjectManagementService = new();
    public ProjectManagementServiceV1Tests()
    {
        _service = new ProjectManagementServiceV1(s_logger, _mockProjectManagementService.Object);
    }

    [Theory]
    [InlineData(2, true)]
    [InlineData(2, false)]
    public async ValueTask Test_GetProjectMetas(int expectedMetasCount, bool hasActiveProject)
    {
        // Arrange
        var request = new GetProjectMetasRequest
        {
            AgentUniqueName = "Test"
        };

        var projectId = Guid.NewGuid();
        _mockProjectManagementService.Setup(m => m.GetAllMetasAsync()).ReturnsAsync(new List<ProjectMetaRecord> {
            new ProjectMetaRecord { Id = projectId, ServiceUniqueName = "Test" },
            new ProjectMetaRecord { Id = projectId, ServiceUniqueName = "Test", IsActive = hasActiveProject },
            new ProjectMetaRecord { Id = Guid.NewGuid(), ServiceUniqueName = "Test" }
        });

        // Act
        GetProjectMetasResponse response = await _service.GetProjectMetas(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(expectedMetasCount, response.ProjectMetas.Count);
    }

    [Theory]
    [InlineData(Roles.Administrator, true, false, false)]
    [InlineData(Roles.Engineer, true, false, false)]
    [InlineData(Roles.Reviewer, true, false, false)]
    [InlineData(Roles.Auditor, false, false, false)]
    [InlineData(Roles.Administrator, true, true, false)]
    [InlineData(Roles.Administrator, true, false, true)]
    public async ValueTask Test_ActivateProject(string userRole, bool isAllowed, bool hasInvalidProjectId, bool isChangeFailing)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockProjectManagementService.Setup(m => m.TryChangeActivationStateAsync(It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(new ProjectManagementResult { IsSuccessful = !isChangeFailing });

        var request = new ActivateProjectRequest
        {
            ProjectDbId = hasInvalidProjectId ? "42" : Guid.NewGuid().ToString()
        };

        // Act
        if (!isAllowed)
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.ActivateProject(request, _serverCallContext));
            return;
        }

        if (hasInvalidProjectId || isChangeFailing)
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.ActivateProject(request, _serverCallContext));
            return;
        }

        Google.Protobuf.WellKnownTypes.Empty response = await _service.ActivateProject(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
    }

    [Theory]
    [InlineData(Roles.Administrator, true, false, false)]
    [InlineData(Roles.Engineer, false, false, false)]
    [InlineData(Roles.Reviewer, true, false, false)]
    [InlineData(Roles.Auditor, false, false, false)]
    [InlineData(Roles.Administrator, true, true, false)]
    [InlineData(Roles.Administrator, true, false, true)]
    public async ValueTask Test_ApproveProject(string userRole, bool isAllowed, bool hasInvalidProjectId, bool isSaveFailing)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockProjectManagementService.Setup(m => m.TrySaveNewVersionAsync(It.IsAny<Guid>(),
                                                                            It.IsAny<ProjectState>(),
                                                                            It.IsAny<string>(),
                                                                            It.IsAny<string>(),
                                                                            It.IsAny<string>()))
                                                                            .ReturnsAsync(new ProjectManagementResult { IsSuccessful = !isSaveFailing });

        var request = new ApproveProjectRequest
        {
            ProjectDbId = hasInvalidProjectId ? "42" : Guid.NewGuid().ToString(),
            ProjectSaveInfo = new ProjectSaveInfo()
        };

        // Act
        if (!isAllowed)
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.ApproveProject(request, _serverCallContext));
            return;
        }

        if (hasInvalidProjectId || isSaveFailing)
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.ApproveProject(request, _serverCallContext));
            return;
        }

        Google.Protobuf.WellKnownTypes.Empty response = await _service.ApproveProject(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, false)]
    [InlineData(Roles.Auditor, false)]
    public async ValueTask Test_CreateProject(string userRole, bool isAllowed)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockProjectManagementService.Setup(m => m.CreateAsync(It.IsAny<string>())).ReturnsAsync(new ProjectRecord());

        var request = new CreateProjectRequest
        {
            ProjectName = "Test"
        };

        // Act
        if (!isAllowed)
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.CreateProject(request, _serverCallContext));
            return;
        }

        CreateProjectResponse response = await _service.CreateProject(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
    }

    [Theory]
    [InlineData(Roles.Administrator, true, false, false)]
    [InlineData(Roles.Engineer, true, false, false)]
    [InlineData(Roles.Reviewer, false, false, false)]
    [InlineData(Roles.Auditor, false, false, false)]
    [InlineData(Roles.Administrator, true, true, false)]
    [InlineData(Roles.Administrator, true, false, true)]
    public async ValueTask Test_DeleteProject(string userRole, bool isAllowed, bool hasInvalidProjectId, bool isDeleteFailing)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockProjectManagementService.Setup(m => m.TryDeleteAsync(It.IsAny<Guid>())).ReturnsAsync(new ProjectManagementResult { IsSuccessful = !isDeleteFailing });

        var request = new DeleteProjectRequest
        {
            ProjectId = hasInvalidProjectId ? "42" : Guid.NewGuid().ToString()
        };

        // Act
        if (!isAllowed)
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.DeleteProject(request, _serverCallContext));
            return;
        }

        if (hasInvalidProjectId || isDeleteFailing)
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.DeleteProject(request, _serverCallContext));
            return;
        }

        Google.Protobuf.WellKnownTypes.Empty response = await _service.DeleteProject(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
    }

    [Theory]
    [InlineData(Roles.Administrator, true, false, false, ProjectState.Draft, true, false)]
    [InlineData(Roles.Engineer, true, false, false, ProjectState.Draft, true, false)]
    [InlineData(Roles.Reviewer, true, false, false, ProjectState.Draft, true, false)]
    [InlineData(Roles.Auditor, false, false, false, ProjectState.Draft, true, false)]
    [InlineData(Roles.Administrator, true, true, false, ProjectState.Draft, false, false)]
    [InlineData(Roles.Administrator, true, false, true, ProjectState.Draft, true, false)]
    [InlineData(Roles.Administrator, true, false, false, ProjectState.Review, true, false)]
    [InlineData(Roles.Administrator, true, false, true, ProjectState.Draft, false, false)]
    [InlineData(Roles.Administrator, true, false, false, ProjectState.Review, false, false)]
    [InlineData(Roles.Administrator, true, false, false, ProjectState.Draft, false, true)]
    public async ValueTask Test_SaveProject(string userRole, bool isAllowed, bool hasInvalidProjectId, bool isSaveFailing, ProjectState projectState, bool hasActiveProject, bool hasEmptyProjectId)
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockProjectManagementService.Setup(m => m.TrySaveNewVersionAsync(It.IsAny<Guid>(),
                                                                            It.IsAny<ProjectState>(),
                                                                            It.IsAny<string>(),
                                                                            It.IsAny<string>(),
                                                                            It.IsAny<string>()))
                                                                            .ReturnsAsync(new ProjectManagementResult { IsSuccessful = !isSaveFailing });
        _mockProjectManagementService.Setup(m => m.TrySaveActiveAsync()).ReturnsAsync(new ProjectManagementResult { IsSuccessful = !isSaveFailing });
        _mockProjectManagementService.Setup(m => m.ActiveProjectId).Returns(projectId);
        _mockProjectManagementService.Setup(m => m.GetAllMetasAsync()).ReturnsAsync(new List<ProjectMetaRecord> {
            new ProjectMetaRecord { Id = projectId, ServiceUniqueName = "Test", IsActive = hasActiveProject, State = projectState }
        });

        var request = new SaveProjectRequest
        {
            ProjectId = hasEmptyProjectId ? string.Empty : hasActiveProject ? projectId.ToString() : hasInvalidProjectId ? "42" : Guid.NewGuid().ToString(),
            ProjectDbId = Guid.NewGuid().ToString(),
            ProjectSaveInfo = new ProjectSaveInfo { State = (int)projectState }
        };

        // Act
        if (!isAllowed)
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.SaveProject(request, _serverCallContext));
            return;
        }

        if (hasInvalidProjectId || isSaveFailing)
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.SaveProject(request, _serverCallContext));
            return;
        }

        Google.Protobuf.WellKnownTypes.Empty response = await _service.SaveProject(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
    }
}
