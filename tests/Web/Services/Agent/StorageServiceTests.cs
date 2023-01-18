using Ayborg.Gateway.Agent.V1;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Services.AppState;
using AyBorg.Web.Shared.Models;
using AyBorg.Web.Tests.Helpers;
using Grpc.Core;
using Moq;

namespace AyBorg.Web.Tests.Services.Agent;

public class StorageServiceTest
{
    private readonly Mock<IStateService> _mockStateService = new();
    private readonly Mock<Storage.StorageClient> _mockStorageClient = new();
    private readonly StorageService _service;

    public StorageServiceTest()
    {
        _mockStateService.Setup(m => m.AgentState).Returns(new UiAgentState
        {
            UniqueName = "Test"
        });

        _service = new StorageService(_mockStateService.Object, _mockStorageClient.Object);
    }

    [Fact]
    public async ValueTask Test_GetDirectoriesAsync()
    {
        // Arrange
        var response = new GetDirectoriesResponse();
        response.Directories.Add("Path-1");
        response.Directories.Add("Path-2");
        AsyncUnaryCall<GetDirectoriesResponse> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
        _mockStorageClient.Setup(m => m.GetDirectoriesAsync(It.IsAny<GetDirectoriesRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        // Act
        IEnumerable<string> result = await _service.GetDirectoriesAsync("/");

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains("Path-1", result);
        Assert.Contains("Path-2", result);
    }
}
