using System.Security.Claims;
using Ayborg.Gateway.Agent.V1;
using AyBorg.Gateway.Services.Agent;
using AyBorg.Gateway.Tests.Helpers;
using AyBorg.SDK.Authorization;
using Grpc.Core;
using Moq;

namespace AyBorg.Gateway.Tests.Services.Agent;

public class RuntimePassthroughServiceV1Tests : BaseGrpcServiceTests<RuntimePassthroughServiceV1, Runtime.RuntimeClient>
{
    public RuntimePassthroughServiceV1Tests()
    {
        _service = new RuntimePassthroughServiceV1(_mockGrpcChannelService.Object);
    }

    [Fact]
    public async Task Test_GetStatus()
    {
        // Arrange
        AsyncUnaryCall<GetRuntimeStatusResponse> mockCallGetAvailableSteps = GrpcCallHelpers.CreateAsyncUnaryCall(new GetRuntimeStatusResponse());
        _mockClient.Setup(c => c.GetStatusAsync(It.IsAny<GetRuntimeStatusRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(mockCallGetAvailableSteps);
        var request = new GetRuntimeStatusRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        GetRuntimeStatusResponse resultResponse = await _service.GetStatus(request, _serverCallContext);

        // Assert
        Assert.NotNull(resultResponse);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, true)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_StartRun(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<StartRunResponse> mockCallActivateProject = GrpcCallHelpers.CreateAsyncUnaryCall(new StartRunResponse());
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockClient.Setup(c => c.StartRunAsync(It.IsAny<StartRunRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallActivateProject);
        var request = new StartRunRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        StartRunResponse resultResponse = null!;
        if (isAllowed)
        {
            resultResponse = await _service.StartRun(request, _serverCallContext);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.StartRun(request, _serverCallContext));
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
    public async Task Test_StopRun(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<StopRunResponse> mockCallActivateProject = GrpcCallHelpers.CreateAsyncUnaryCall(new StopRunResponse());
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockClient.Setup(c => c.StopRunAsync(It.IsAny<StopRunRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallActivateProject);
        var request = new StopRunRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        StopRunResponse resultResponse = null!;
        if (isAllowed)
        {
            resultResponse = await _service.StopRun(request, _serverCallContext);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.StopRun(request, _serverCallContext));
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
    public async Task Test_AbortRun(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<AbortRunResponse> mockCallActivateProject = GrpcCallHelpers.CreateAsyncUnaryCall(new AbortRunResponse());
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockClient.Setup(c => c.AbortRunAsync(It.IsAny<AbortRunRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallActivateProject);
        var request = new AbortRunRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        AbortRunResponse resultResponse = null!;
        if (isAllowed)
        {
            resultResponse = await _service.AbortRun(request, _serverCallContext);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.AbortRun(request, _serverCallContext));
            return;
        }

        // Assert
        Assert.NotNull(resultResponse);
    }
}
