/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Ayborg.Gateway.Agent.V1;
using AyBorg.Communication.gRPC;
using AyBorg.Types.Models;
using AyBorg.Types.Ports;
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
        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<StepDto>())).Returns(new StepModel
        {
            Ports = new List<PortModel> {
                new() {
                    Brand = PortBrand.Image,
                    Direction = PortDirection.Input
                    },
                new() {
                    Brand = PortBrand.Numeric
                }
            }
        });

        // Act
        IEnumerable<StepModel> result = await _service.GetStepsAsync();

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
        getFlowPortsResponse.Ports.Add(new PortDto
        {
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
        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<StepDto>())).Returns(new StepModel
        {
            Ports = [
                new() {
                    Brand = PortBrand.Image,
                    Direction = PortDirection.Input
                    },
                new() {
                    Brand = PortBrand.Numeric
                }
            ]
        });
        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<PortDto>())).Returns(new PortModel { Brand = PortBrand.Image });


        // Act
        StepModel result = await _service.GetStepAsync(string.Empty, new StepModel(), Guid.NewGuid(), updatePorts, skipOutputPorts);

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
    public async Task Test_GetStepAsync_RpcException()
    {
        // Arrange
        _mockEditorClient.Setup(m => m.GetFlowStepsAsync(It.IsAny<GetFlowStepsRequest>(), null, null, It.IsAny<CancellationToken>())).Throws(new RpcException(Status.DefaultCancelled));

        // Act
        StepModel result = await _service.GetStepAsync(string.Empty, new StepModel(), Guid.NewGuid(), false, false);

        // Assert
        Assert.Equal(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task Test_GetLinksAsync()
    {
        // Arrange
        var getFlowLinksResponse = new GetFlowLinksResponse();
        getFlowLinksResponse.Links.Add(new LinkDto());
        AsyncUnaryCall<GetFlowLinksResponse> callGetFlowLinks = GrpcCallHelpers.CreateAsyncUnaryCall(getFlowLinksResponse);
        _mockEditorClient.Setup(m => m.GetFlowLinksAsync(It.IsAny<GetFlowLinksRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(callGetFlowLinks);

        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<LinkDto>())).Returns(new LinkModel());

        // Act
        IEnumerable<LinkModel> result = await _service.GetLinksAsync();

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

        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<LinkDto>())).Returns(new LinkModel { Id = Guid.NewGuid() });

        // Act
        LinkModel result = await _service.GetLinkAsync(Guid.NewGuid());

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

        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<StepDto>())).Returns(new StepModel());

        // Act
        StepModel result = await _service.AddStepAsync(new StepModel());

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Test_AddStepAsync_RpcException()
    {
        // Arrange
        _mockEditorClient.Setup(m => m.AddFlowStepAsync(It.IsAny<AddFlowStepRequest>(), null, null, It.IsAny<CancellationToken>())).Throws(new RpcException(Status.DefaultCancelled));

        // Act
        StepModel result = await _service.AddStepAsync(new StepModel());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Test_TryRemoveStepAsync()
    {
        // Arrange
        AsyncUnaryCall<Empty> call = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
        _mockEditorClient.Setup(m => m.DeleteFlowStepAsync(It.IsAny<DeleteFlowStepRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        // Act
        bool result = await _service.TryRemoveStepAsync(new StepModel());

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

        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<LinkDto>())).Returns(new LinkModel());

        // Act
        Guid? result = await _service.AddLinkAsync(new PortModel(), new PortModel());

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

        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<LinkDto>())).Returns(new LinkModel());

        // Act
        bool result = await _service.TryRemoveLinkAsync(new LinkModel());

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

        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<PortDto>())).Returns(new PortModel() { Id = Guid.NewGuid() });

        // Act
        PortModel result = await _service.GetPortAsync(string.Empty, Guid.NewGuid(), Guid.NewGuid());

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
    public async Task Test_GetPortAsync_RpcException()
    {
        // Arrange
        _mockEditorClient.Setup(m => m.GetFlowPortsAsync(It.IsAny<GetFlowPortsRequest>(), null, null, It.IsAny<CancellationToken>())).Throws(new RpcException(Status.DefaultCancelled));

        // Act
        PortModel result = await _service.GetPortAsync(string.Empty, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Equal(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task Test_TrySetPortValueAsync()
    {
        // Arrange
        AsyncUnaryCall<Empty> call = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
        _mockEditorClient.Setup(m => m.UpdateFlowPortAsync(It.IsAny<UpdateFlowPortRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        _mockRpcMapper.Setup(m => m.FromRpc(It.IsAny<PortDto>())).Returns(new PortModel());
        bool called = false;
        _service.PortValueChanged += (s, e) =>
        {
            called = true;
        };

        // Act
        bool result = await _service.TrySetPortValueAsync(new PortModel());

        // Assert
        Assert.True(result);
        Assert.True(called);
    }
}
