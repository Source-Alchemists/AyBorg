using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Tests.Helpers;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Web.Tests.Services.Agent;

public class PluginsServiceTests
{
    private static readonly NullLogger<PluginsService> s_logger = new();
    private readonly Mock<IRpcMapper> _mockRpcMapper = new();
    private readonly Mock<Editor.EditorClient> _mockEditorClient = new();
    private readonly PluginsService _service;

    public PluginsServiceTests()
    {
        _service = new PluginsService(s_logger, _mockRpcMapper.Object, _mockEditorClient.Object);
    }

    [Fact]
    public async Task Test_ReceiveStepsAsync()
    {
        // Arrange
        var steps = new List<StepDto>
        {
            new StepDto(),
            new StepDto(),
        };
        var response = new GetAvailableStepsResponse { Steps = { steps } };
        AsyncUnaryCall<GetAvailableStepsResponse> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
        _mockEditorClient.Setup(x => x.GetAvailableStepsAsync(It.IsAny<GetAvailableStepsRequest>(), null, null, default)).Returns(call);
        _mockRpcMapper.Setup(x => x.FromRpc(It.IsAny<StepDto>())).Returns(new Step());

        // Act
        IEnumerable<Step> actual = await _service.ReceiveStepsAsync("Test");

        // Assert
        Assert.Equal(2, actual.Count());
    }
}
