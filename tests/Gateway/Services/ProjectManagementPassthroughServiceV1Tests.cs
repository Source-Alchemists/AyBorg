using System.Security.Claims;
using Ayborg.Gateway.Agent.V1;
using AyBorg.Gateway.Services.Agent;
using AyBorg.Gateway.Tests.Helpers;
using AyBorg.SDK.Authorization;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Moq;

namespace AyBorg.Gateway.Tests.Services;

public class ProjectManagementPassthroughServiceV1Tests : BaseGrpcServiceTests<ProjectManagementPassthroughServiceV1, ProjectManagement.ProjectManagementClient>
{
    public ProjectManagementPassthroughServiceV1Tests()
    {
        _service = new ProjectManagementPassthroughServiceV1(s_logger, _mockGrpcChannelService.Object);
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
