using System.Security.Claims;
using Ayborg.Gateway.Agent.V1;
using AyBorg.Agent.Services;
using AyBorg.Agent.Services.gRPC;
using AyBorg.SDK.Authorization;
using Moq;

namespace AyBorg.Agent.Tests.Services.gRPC;

public class StorageServiceV1Tests : BaseGrpcServiceTests<StorageServiceV1, Storage.StorageClient>
{
    private readonly Mock<IStorageService> _mockStorageService = new();

    public StorageServiceV1Tests()
    {
        _service = new StorageServiceV1(_mockStorageService.Object);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, true)]
    [InlineData(Roles.Auditor, true)]
    [InlineData("", false)]
    public async ValueTask Test_GetDirectories(string userRole, bool isAllowed)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockStorageService.Setup(m => m.GetDirectories(It.IsAny<string>())).Returns(new List<string> { "/Test " });
        var request = new GetDirectoriesRequest
        {
            Path = "/"
        };

        // Act
        if (!isAllowed)
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetDirectories(request, _serverCallContext));
            return;
        }

        GetDirectoriesResponse response = await _service.GetDirectories(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Directories);
    }
}
