using System.Security.Claims;
using Ayborg.Gateway.Agent.V1;
using AyBorg.Gateway.Services.Tests;
using AyBorg.Gateway.Tests.Helpers;
using AyBorg.SDK.Authorization;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Moq;

namespace AyBorg.Gateway.Services.Agent.Tests;

public class ProjectSettingsPassthroughServiceV1Test : BaseGrpcServiceTests<ProjectSettingsPassthroughServiceV1, ProjectSettings.ProjectSettingsClient>
{
    public ProjectSettingsPassthroughServiceV1Test()
    {
        _service = new ProjectSettingsPassthroughServiceV1(_mockGrpcChannelService.Object);
    }

    [Fact]
    public async Task Test_GetProjectSettings()
    {
        // Arrange
        AsyncUnaryCall<GetProjectSettingsResponse> mockCallGetProjectSettings = GrpcCallHelpers.CreateAsyncUnaryCall(new GetProjectSettingsResponse());
        _mockClient.Setup(c => c.GetProjectSettingsAsync(It.IsAny<GetProjectSettingsRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(mockCallGetProjectSettings);
        var request = new GetProjectSettingsRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        GetProjectSettingsResponse resultResponse = await _service.GetProjectSettings(request, _serverCallContext);

        // Assert
        Assert.NotNull(resultResponse);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, false)]
    [InlineData(Roles.Reviewer, false)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_UpdateProjectSettings(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<Empty> mockCallUpdateProjectSettings = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockClient.Setup(c => c.UpdateProjectSettingsAsync(It.IsAny<UpdateProjectSettingsRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallUpdateProjectSettings);
        var request = new UpdateProjectSettingsRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        Empty resultResponse = null!;
        if (isAllowed)
        {
            resultResponse = await _service.UpdateProjectSettings(request, _serverCallContext);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _service.UpdateProjectSettings(request, _serverCallContext));
            return;
        }

        // Assert
        Assert.NotNull(resultResponse);

    }
}
