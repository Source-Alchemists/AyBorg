using System.Security.Principal;
using Ayborg.Gateway.Agent.V1;
using AyBorg.Web.Shared.Models;
using AyBorg.Web.Tests.Helpers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Web.Services.Agent.Tests;

public class ProjectManagementServiceTests
{
    private static readonly NullLogger<ProjectManagementService> s_logger = new();
    private readonly Mock<IStateService> _mockStateService = new();
    private readonly Mock<AuthenticationStateProvider> _mockAuthenticationStateProvider = new();
    private readonly Mock<ProjectManagement.ProjectManagementClient> _mockProjectManagementClient = new();
    private readonly ProjectManagementService _service;

    public ProjectManagementServiceTests()
    {
        _mockStateService.Setup(m => m.AgentState).Returns(new UiAgentState
        {
            UniqueName = "Test"
        });

        var mockIdentity = new Mock<IIdentity>();
        mockIdentity.Setup(m => m.Name).Returns("AdminUser");
        _mockAuthenticationStateProvider.Setup(m => m.GetAuthenticationStateAsync()).ReturnsAsync(new AuthenticationState(new System.Security.Claims.ClaimsPrincipal(mockIdentity.Object)));

        _service = new ProjectManagementService(s_logger, _mockStateService.Object, _mockAuthenticationStateProvider.Object, _mockProjectManagementClient.Object);
    }

    [Fact]
    public async ValueTask Test_GetMetasAsync()
    {
        // Arrange
        var response = new GetProjectMetasResponse();
        response.ProjectMetas.Add(new ProjectMeta
        {
            CreationDate = new Timestamp(),
            ChangeDate = new Timestamp()
        });
        AsyncUnaryCall<GetProjectMetasResponse> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
        _mockProjectManagementClient.Setup(m => m.GetProjectMetasAsync(It.IsAny<GetProjectMetasRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        // Act
        IEnumerable<Shared.Models.Agent.ProjectMeta> result = await _service.GetMetasAsync();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async ValueTask Test_GetMetasAsync_Failed()
    {
        // Arrange
        _mockProjectManagementClient.Setup(m => m.GetProjectMetasAsync(It.IsAny<GetProjectMetasRequest>(), null, null, It.IsAny<CancellationToken>())).Throws(new RpcException(Status.DefaultCancelled));

        // Act
        IEnumerable<Shared.Models.Agent.ProjectMeta> result = await _service.GetMetasAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async ValueTask Test_GetActiveMetaAsync()
    {
        // Arrange
        var response = new GetProjectMetasResponse();
        response.ProjectMetas.Add(new ProjectMeta
        {
            CreationDate = new Timestamp(),
            ChangeDate = new Timestamp(),
            IsActive = false
        });
        response.ProjectMetas.Add(new ProjectMeta
        {
            CreationDate = new Timestamp(),
            ChangeDate = new Timestamp(),
            IsActive = true
        });
        AsyncUnaryCall<GetProjectMetasResponse> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
        _mockProjectManagementClient.Setup(m => m.GetProjectMetasAsync(It.IsAny<GetProjectMetasRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        // Act
        Shared.Models.Agent.ProjectMeta result = await _service.GetActiveMetaAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async ValueTask Test_GetActiveMetaAsync_Failed()
    {
        // Arrange
        _mockProjectManagementClient.Setup(m => m.GetProjectMetasAsync(It.IsAny<GetProjectMetasRequest>(), null, null, It.IsAny<CancellationToken>())).Throws(new RpcException(Status.DefaultCancelled));

        // Act
        Shared.Models.Agent.ProjectMeta result = await _service.GetActiveMetaAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async ValueTask Test_()
    {
        // Arrange
        var response = new CreateProjectResponse
        {
            ProjectMeta = new ProjectMeta
            {
                CreationDate = new Timestamp(),
                ChangeDate = new Timestamp()
            }
        };
        AsyncUnaryCall<CreateProjectResponse> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
        _mockProjectManagementClient.Setup(m => m.CreateProjectAsync(It.IsAny<CreateProjectRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        // Act
        Shared.Models.Agent.ProjectMeta result = await _service.CreateAsync("Test");

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async ValueTask Test_CreateAsync_Failed()
    {
        // Arrange
        _mockProjectManagementClient.Setup(m => m.CreateProjectAsync(It.IsAny<CreateProjectRequest>(), null, null, It.IsAny<CancellationToken>())).Throws(new RpcException(Status.DefaultCancelled));

        // Act
        Shared.Models.Agent.ProjectMeta result = await _service.CreateAsync("Test");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async ValueTask Test_TryDeleteAsync()
    {
        // Arrange
        var response = new Empty();
        AsyncUnaryCall<Empty> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
        _mockProjectManagementClient.Setup(m => m.DeleteProjectAsync(It.IsAny<DeleteProjectRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        // Act
        bool result = await _service.TryDeleteAsync(new Shared.Models.Agent.ProjectMeta());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async ValueTask Test_TryDeleteAsync_Failed()
    {
        // Arrange
        _mockProjectManagementClient.Setup(m => m.DeleteProjectAsync(It.IsAny<DeleteProjectRequest>(), null, null, It.IsAny<CancellationToken>())).Throws(new RpcException(Status.DefaultCancelled));

        // Act
        bool result = await _service.TryDeleteAsync(new Shared.Models.Agent.ProjectMeta());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async ValueTask Test_TryActivateAsync()
    {
        // Arrange
        var response = new Empty();
        AsyncUnaryCall<Empty> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
        _mockProjectManagementClient.Setup(m => m.ActivateProjectAsync(It.IsAny<ActivateProjectRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        // Act
        bool result = await _service.TryActivateAsync(new Shared.Models.Agent.ProjectMeta());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async ValueTask Test_TryActivateAsync_Failed()
    {
        // Arrange
        _mockProjectManagementClient.Setup(m => m.ActivateProjectAsync(It.IsAny<ActivateProjectRequest>(), null, null, It.IsAny<CancellationToken>())).Throws(new RpcException(Status.DefaultCancelled));

        // Act
        bool result = await _service.TryActivateAsync(new Shared.Models.Agent.ProjectMeta());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async ValueTask Test_TrySaveAsync()
    {
        // Arrange
        var response = new Empty();
        AsyncUnaryCall<Empty> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
        _mockProjectManagementClient.Setup(m => m.SaveProjectAsync(It.IsAny<SaveProjectRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        // Act
        bool result = await _service.TrySaveAsync(new Shared.Models.Agent.ProjectMeta(), new Shared.Models.Agent.ProjectSaveInfo
        {
            State = SDK.Projects.ProjectState.Draft,
            VersionName = "123",
            Comment = string.Empty
        });

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async ValueTask Test_TrySaveAsync_Failed()
    {
        // Arrange
        _mockProjectManagementClient.Setup(m => m.SaveProjectAsync(It.IsAny<SaveProjectRequest>(), null, null, It.IsAny<CancellationToken>())).Throws(new RpcException(Status.DefaultCancelled));

        // Act
        bool result = await _service.TrySaveAsync(new Shared.Models.Agent.ProjectMeta(), new Shared.Models.Agent.ProjectSaveInfo
        {
            State = SDK.Projects.ProjectState.Draft,
            VersionName = "123",
            Comment = string.Empty
        });

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async ValueTask Test_TryApproveAsync()
    {
        // Arrange
        var response = new Empty();
        AsyncUnaryCall<Empty> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
        _mockProjectManagementClient.Setup(m => m.ApproveProjectAsync(It.IsAny<ApproveProjectRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        // Act
        bool result = await _service.TryApproveAsync(new Shared.Models.Agent.ProjectMeta(), new Shared.Models.Agent.ProjectSaveInfo
        {
            State = SDK.Projects.ProjectState.Review,
            VersionName = "123",
            Comment = string.Empty
        });

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async ValueTask Test_TryApproveAsync_Failed()
    {
        // Arrange
        _mockProjectManagementClient.Setup(m => m.ApproveProjectAsync(It.IsAny<ApproveProjectRequest>(), null, null, It.IsAny<CancellationToken>())).Throws(new RpcException(Status.DefaultCancelled));

        // Act
        bool result = await _service.TryApproveAsync(new Shared.Models.Agent.ProjectMeta(), new Shared.Models.Agent.ProjectSaveInfo
        {
            State = SDK.Projects.ProjectState.Draft,
            VersionName = "123",
            Comment = string.Empty
        });

        // Assert
        Assert.False(result);
    }
}
