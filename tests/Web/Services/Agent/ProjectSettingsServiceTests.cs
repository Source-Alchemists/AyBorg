using Ayborg.Gateway.Agent.V1;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Tests.Helpers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Web.Tests.Services.Agent;

public class ProjectSettingsServiceTests
{
    private static readonly NullLogger<ProjectSettingsService> s_logger = new();
    private readonly Mock<ProjectSettings.ProjectSettingsClient> _mockProjectSettingsClient = new();
    private readonly ProjectSettingsService _service;

    public ProjectSettingsServiceTests()
    {
        _service = new ProjectSettingsService(s_logger, _mockProjectSettingsClient.Object);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async ValueTask Test_GetProjectSettingsAsync(bool isSuccessful)
    {
        // Arrange
        if (isSuccessful)
        {
            var response = new GetProjectSettingsResponse {
                ProjectSettings = new ProjectSettingsDto()
            };
            AsyncUnaryCall<GetProjectSettingsResponse> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
            _mockProjectSettingsClient.Setup(m => m.GetProjectSettingsAsync(It.IsAny<GetProjectSettingsRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);
        }
        else
        {
            _mockProjectSettingsClient.Setup(m => m.GetProjectSettingsAsync(It.IsAny<GetProjectSettingsRequest>(), null, null, It.IsAny<CancellationToken>())).Throws(new RpcException(Status.DefaultCancelled));
        }

        // Act
        Shared.Models.Agent.ProjectSettings result = await _service.GetProjectSettingsAsync("Agent.UniqueName.Test", new Shared.Models.Agent.ProjectMeta());

        // Assert
        if (isSuccessful)
        {
            Assert.NotNull(result);
        }
        else
        {
            Assert.Null(result);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async ValueTask Test_TryUpdateProjectCommunicationSettingsAsync(bool isSuccessful)
    {
        // Arrange
        if (isSuccessful)
        {
            var response = new Empty();
            AsyncUnaryCall<Empty> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
            _mockProjectSettingsClient.Setup(m => m.UpdateProjectSettingsAsync(It.IsAny<UpdateProjectSettingsRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);
        }
        else
        {
            _mockProjectSettingsClient.Setup(m => m.UpdateProjectSettingsAsync(It.IsAny<UpdateProjectSettingsRequest>(), null, null, It.IsAny<CancellationToken>())).Throws(new RpcException(Status.DefaultCancelled));
        }

        // Act
        bool result = await _service.TryUpdateProjectCommunicationSettingsAsync("Agent.UniqueName.Test", new Shared.Models.Agent.ProjectMeta(), new Shared.Models.Agent.ProjectSettings(new ProjectSettingsDto()));

        // Assert
        Assert.Equal(isSuccessful, result);
    }
}
