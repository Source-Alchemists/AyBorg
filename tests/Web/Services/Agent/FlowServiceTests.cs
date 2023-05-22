using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Shared.Models;
using AyBorg.Web.Tests.Helpers;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
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
        _mockStateService.Setup(m => m.AgentState).Returns(new UiAgentState
        {
            UniqueName = "Test"
        });

        _service = new FlowService(s_logger, _mockStateService.Object, _mockRpcMapper.Object, _mockEditorClient.Object);
    }

    [Fact]
    public async Task Test_GetStepsAsync()
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
    public async Task Test_GetStepAsync(bool hasStep, bool updatePorts, bool skipOutputPorts)
    {
        // Arrange
        var getFlowStepsResponse = new GetFlowStepsResponse();
        if (hasStep)
        {
            getFlowStepsResponse.Steps.Add(new StepDto());
        }

        var getFlowPortsResponse = new GetFlowPortsResponse();
        getFlowPortsResponse.Ports.Add(new PortDto {
            Id = Guid.NewGuid().ToString(),
            Name = "Test_name",
            Direction = (int)PortDirection.Output,
            IsConnected = false,
            IsLinkConvertable = true,
            Brand = (int)PortBrand.String,
            Value = "Test"
        });

        AsyncUnaryCall<GetFlowStepsResponse> callGetFlowSteps = GrpcCallHelpers.CreateAsyncUnaryCall(getFlowStepsResponse);
        AsyncUnaryCall<GetFlowPortsResponse> callGetFlowPorts = GrpcCallHelpers.CreateAsyncUnaryCall(getFlowPortsResponse);
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
        _mockEditorClient.Setup(m => m.GetFlowPortsAsync(It.IsAny<GetFlowPortsRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(callGetFlowPorts);
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
        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<PortDto>())).Returns(new Port { Brand = PortBrand.Image });


        // Act
        Step result = await _service.GetStepAsync(new Step(), Guid.NewGuid(), updatePorts, skipOutputPorts);

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
            Assert.Equal(Guid.Empty, result.Id);
        }
    }

    [Fact]
    public async Task Test_GetLinksAsync()
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
    public async Task Test_GetLinkAsync(bool hasLink)
    {
        // Arrange
        var getFlowLinksResponse = new GetFlowLinksResponse();
        if (hasLink)
        {
            getFlowLinksResponse.Links.Add(new LinkDto());
        }

        AsyncUnaryCall<GetFlowLinksResponse> callGetFlowLinks = GrpcCallHelpers.CreateAsyncUnaryCall(getFlowLinksResponse);
        _mockEditorClient.Setup(m => m.GetFlowLinksAsync(It.IsAny<GetFlowLinksRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(callGetFlowLinks);

        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<LinkDto>())).Returns(new Link { Id = Guid.NewGuid() });

        // Act
        Link result = await _service.GetLinkAsync(Guid.NewGuid());

        // Assert
        if (hasLink)
        {
            Assert.NotEqual(Guid.Empty, result.Id);
        }
        else
        {
            Assert.Equal(Guid.Empty, result.Id);
        }
    }

    [Fact]
    public async Task Test_AddStepAsync()
    {
        // Arrange
        AsyncUnaryCall<AddFlowStepResponse> call = GrpcCallHelpers.CreateAsyncUnaryCall(new AddFlowStepResponse());
        _mockEditorClient.Setup(m => m.AddFlowStepAsync(It.IsAny<AddFlowStepRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<StepDto>())).Returns(new Step());

        // Act
        Step result = await _service.AddStepAsync(new Step());

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Test_TryRemoveStepAsync()
    {
        // Arrange
        AsyncUnaryCall<Empty> call = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
        _mockEditorClient.Setup(m => m.DeleteFlowStepAsync(It.IsAny<DeleteFlowStepRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        // Act
        bool result = await _service.TryRemoveStepAsync(new Step());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Test_TryMoveStepAsync()
    {
        // Arrange
        AsyncUnaryCall<Empty> call = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
        _mockEditorClient.Setup(m => m.MoveFlowStepAsync(It.IsAny<MoveFlowStepRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        // Act
        bool result = await _service.TryMoveStepAsync(Guid.NewGuid(), 0, 0);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Test_AddLinkAsync(bool notLinking)
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        AsyncUnaryCall<LinkFlowPortsResponse> callGetFlowLinks = GrpcCallHelpers.CreateAsyncUnaryCall(new LinkFlowPortsResponse
        {
            LinkId = notLinking ? "42" : expectedId.ToString()
        });
        _mockEditorClient.Setup(m => m.LinkFlowPortsAsync(It.IsAny<LinkFlowPortsRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(callGetFlowLinks);

        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<LinkDto>())).Returns(new Link());

        // Act
        Guid? result = await _service.AddLinkAsync(new Port(), new Port());

        // Assert
        if (!notLinking)
        {
            Assert.Equal(expectedId, result);
        }
        else
        {
            Assert.Equal(Guid.Empty, result);
        }
    }

    [Fact]
    public async Task Test_TryRemoveLinkAsync()
    {
        // Arrange
        AsyncUnaryCall<LinkFlowPortsResponse> callGetFlowLinks = GrpcCallHelpers.CreateAsyncUnaryCall(new LinkFlowPortsResponse());
        _mockEditorClient.Setup(m => m.LinkFlowPortsAsync(It.IsAny<LinkFlowPortsRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(callGetFlowLinks);

        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<LinkDto>())).Returns(new Link());

        // Act
        bool result = await _service.TryRemoveLinkAsync(new Link());

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Test_GetPortAsync(bool hasPort)
    {
        // Arrange
        var response = new GetFlowPortsResponse();
        if (hasPort)
        {
            response.Ports.Add(new PortDto() { Id = Guid.NewGuid().ToString() });
        }
        AsyncUnaryCall<GetFlowPortsResponse> call = GrpcCallHelpers.CreateAsyncUnaryCall(response);
        _mockEditorClient.Setup(m => m.GetFlowPortsAsync(It.IsAny<GetFlowPortsRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<PortDto>())).Returns(new Port() { Id = Guid.NewGuid() });

        // Act
        Port result = await _service.GetPortAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        if (!hasPort)
        {
            Assert.Equal(Guid.Empty, result.Id);
        }
        else
        {
            Assert.NotEqual(Guid.Empty, result.Id);
        }
    }

    [Fact]
    public async Task Test_TrySetPortValueAsync()
    {
        // Arrange
        AsyncUnaryCall<Empty> call = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
        _mockEditorClient.Setup(m => m.UpdateFlowPortAsync(It.IsAny<UpdateFlowPortRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<PortDto>())).Returns(new Port());

        // Act
        bool result = await _service.TrySetPortValueAsync(new Port());

        // Assert
        Assert.True(result);
    }
}
