using System.Security.Claims;
using Ayborg.Gateway.Agent.V1;
using AyBorg.Gateway.Services.Agent;
using AyBorg.Gateway.Tests.Helpers;
using AyBorg.SDK.Authorization;
using Grpc.Core;
using Moq;

namespace AyBorg.Gateway.Tests.Services;

public class StoragePassthroughServiceV1Tests : BaseGrpcServiceTests<StoragePassthroughServiceV1, Storage.StorageClient>
{
    public StoragePassthroughServiceV1Tests()
    {
        _service = new StoragePassthroughServiceV1(s_logger, _mockGrpcChannelService.Object);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, true)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_GetDirectories(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<GetDirectoriesResponse> mockCallActivateProject = GrpcCallHelpers.CreateAsyncUnaryCall(new GetDirectoriesResponse());
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockClient.Setup(c => c.GetDirectoriesAsync(It.IsAny<GetDirectoriesRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallActivateProject);
        var request = new GetDirectoriesRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        GetDirectoriesResponse resultResponse = null!;
        if (isAllowed)
        {
            resultResponse = await _service.GetDirectories(request, _serverCallContext);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetDirectories(request, _serverCallContext));
            return;
        }

        // Assert
        Assert.NotNull(resultResponse);
    }
}
