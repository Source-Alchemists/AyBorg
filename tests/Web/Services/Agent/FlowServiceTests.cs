using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Services.AppState;
using AyBorg.Web.Shared.Models;
using AyBorg.Web.Tests.Helpers;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Web.Tests.Services.Agent;

public class FlowServiceTests
{
    private static readonly NullLogger<FlowService> s_logger = new();
    private readonly Mock<IStateService> _mockStateService = new();
    private readonly Mock<IRpcMapper> _mockRpcMapper = new();
    private readonly Mock<Editor.EditorClient> _mockEditorClient = new();

    private readonly FlowService _service;

    public FlowServiceTests()
    {
        _mockStateService.Setup(m => m.AgentState).Returns(new Shared.Models.UiAgentState
        {
            UniqueName = "Test"
        });

        _service = new FlowService(s_logger, _mockStateService.Object, _mockRpcMapper.Object, _mockEditorClient.Object);
    }

    [Fact]
    public async ValueTask Test_GetStepsAsync()
    {
        // Arrange
        var getFlowStepsResponse = new GetFlowStepsResponse();
        getFlowStepsResponse.Steps.Add(new StepDto());
        AsyncUnaryCall<GetFlowStepsResponse> callGetFlowSteps = GrpcCallHelpers.CreateAsyncUnaryCall(getFlowStepsResponse);
        AsyncServerStreamingCall<ImageChunkDto> callGetImageStream = GrpcCallHelpers.CreateAsyncServerStreamingCall(new List<ImageChunkDto> {
            new ImageChunkDto {
                Data = ByteString.CopyFromUtf8("Test"),
                FullWidth = 2,
                FullHeight = 2,
                FullStreamLength = 2
            }
        });
        _mockEditorClient.Setup(m => m.GetFlowStepsAsync(It.IsAny<GetFlowStepsRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(callGetFlowSteps);
        _mockEditorClient.Setup(m => m.GetImageStream(It.IsAny<GetImageStreamRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(callGetImageStream);
        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<StepDto>())).Returns(new Step
        {
            Ports = new List<Port> {
                new Port {
                    Brand = PortBrand.Image,
                    Direction = PortDirection.Input
                    },
                new Port {
                    Brand = PortBrand.Numeric
                }
            }
        });

        // Act
        IEnumerable<Step> result = await _service.GetStepsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(2, result.First().Ports!.Count());
        Assert.Single(result.First().Ports!.Where(p => p.Brand == PortBrand.Image));
        Assert.Single(result.First().Ports!.Where(p => p.Brand == PortBrand.Numeric));
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    public async ValueTask Test_GetStepAsync(bool hasStep, bool updatePorts, bool skipOutputPorts)
    {
        // Arrange
        var getFlowStepsResponse = new GetFlowStepsResponse();
        if (hasStep)
        {
            getFlowStepsResponse.Steps.Add(new StepDto());
        }

        AsyncUnaryCall<GetFlowStepsResponse> callGetFlowSteps = GrpcCallHelpers.CreateAsyncUnaryCall(getFlowStepsResponse);
        AsyncServerStreamingCall<ImageChunkDto> callGetImageStream = GrpcCallHelpers.CreateAsyncServerStreamingCall(new List<ImageChunkDto> {
            new ImageChunkDto {
                Data = ByteString.CopyFromUtf8("Test"),
                FullWidth = 2,
                FullHeight = 2,
                FullStreamLength = 2
            }
        });
        _mockEditorClient.Setup(m => m.GetFlowStepsAsync(It.IsAny<GetFlowStepsRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(callGetFlowSteps);
        _mockEditorClient.Setup(m => m.GetImageStream(It.IsAny<GetImageStreamRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(callGetImageStream);
        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<StepDto>())).Returns(new Step
        {
            Ports = new List<Port> {
                new Port {
                    Brand = PortBrand.Image,
                    Direction = PortDirection.Input
                    },
                new Port {
                    Brand = PortBrand.Numeric
                }
            }
        });


        // Act
        Step result = await _service.GetStepAsync(Guid.NewGuid(), Guid.NewGuid(), updatePorts, skipOutputPorts);

        // Assert
        if (hasStep)
        {
            Assert.NotNull(result);
            if (!skipOutputPorts)
            {
                var image = (Image)result.Ports!.First(p => p.Brand == PortBrand.Image).Value!;
                Assert.Equal(2, image.Width);
                Assert.Equal(2, image.Height);
            }
        }
        else
        {
            Assert.Null(result);
        }
    }

    [Fact]
    public async ValueTask Test_GetLinksAsync()
    {
        // Arrange
        var getFlowLinksResponse = new GetFlowLinksResponse();
        getFlowLinksResponse.Links.Add(new LinkDto());
        AsyncUnaryCall<GetFlowLinksResponse> callGetFlowLinks = GrpcCallHelpers.CreateAsyncUnaryCall(getFlowLinksResponse);
        _mockEditorClient.Setup(m => m.GetFlowLinksAsync(It.IsAny<GetFlowLinksRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(callGetFlowLinks);

        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<LinkDto>())).Returns(new Link());

        // Act
        IEnumerable<Link> result = await _service.GetLinksAsync();

        // Assert
        Assert.Single(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async ValueTask Test_GetLinkAsync(bool hasLink)
    {
        // Arrange
        var getFlowLinksResponse = new GetFlowLinksResponse();
        if (hasLink)
        {
            getFlowLinksResponse.Links.Add(new LinkDto());
        }

        AsyncUnaryCall<GetFlowLinksResponse> callGetFlowLinks = GrpcCallHelpers.CreateAsyncUnaryCall(getFlowLinksResponse);
        _mockEditorClient.Setup(m => m.GetFlowLinksAsync(It.IsAny<GetFlowLinksRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(callGetFlowLinks);

        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<LinkDto>())).Returns(new Link());

        // Act
        Link result = await _service.GetLinkAsync(Guid.NewGuid());

        // Assert
        if (hasLink)
        {
            Assert.NotNull(result);
        }
        else
        {
            Assert.Null(result);
        }
    }
}
