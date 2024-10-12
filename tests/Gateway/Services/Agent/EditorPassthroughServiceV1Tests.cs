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

using System.Security.Claims;
using Ayborg.Gateway.Agent.V1;
using AyBorg.Authorization;
using AyBorg.Gateway.Services.Tests;
using AyBorg.Gateway.Tests.Helpers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Moq;

namespace AyBorg.Gateway.Services.Agent.Tests;

public class EditorPassthroughServiceV1Tests : BaseGrpcServiceTests<EditorPassthroughServiceV1, Editor.EditorClient>
{
    public EditorPassthroughServiceV1Tests()
    {
        _service = new EditorPassthroughServiceV1(_mockGrpcChannelService.Object);
    }

    [Fact]
    public async Task Test_GetAvailableSteps()
    {
        // Arrange
        AsyncUnaryCall<GetAvailableStepsResponse> mockCallGetAvailableSteps = GrpcCallHelpers.CreateAsyncUnaryCall(new GetAvailableStepsResponse());
        _mockClient.Setup(c => c.GetAvailableStepsAsync(It.IsAny<GetAvailableStepsRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(mockCallGetAvailableSteps);
        var request = new GetAvailableStepsRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        GetAvailableStepsResponse resultResponse = await _service.GetAvailableSteps(request, _serverCallContext);

        // Assert
        Assert.NotNull(resultResponse);
    }

    [Fact]
    public async Task Test_GetFlowSteps()
    {
        // Arrange
        AsyncUnaryCall<GetFlowStepsResponse> mockCallGetFlowSteps = GrpcCallHelpers.CreateAsyncUnaryCall(new GetFlowStepsResponse());
        _mockClient.Setup(c => c.GetFlowStepsAsync(It.IsAny<GetFlowStepsRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(mockCallGetFlowSteps);
        var request = new GetFlowStepsRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        GetFlowStepsResponse resultResponse = await _service.GetFlowSteps(request, _serverCallContext);

        // Assert
        Assert.NotNull(resultResponse);
    }

    [Fact]
    public async Task Test_GetFlowLinks()
    {
        // Arrange
        AsyncUnaryCall<GetFlowLinksResponse> mockCallGetFlowLinks = GrpcCallHelpers.CreateAsyncUnaryCall(new GetFlowLinksResponse());
        _mockClient.Setup(c => c.GetFlowLinksAsync(It.IsAny<GetFlowLinksRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(mockCallGetFlowLinks);
        var request = new GetFlowLinksRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        GetFlowLinksResponse resultResponse = await _service.GetFlowLinks(request, _serverCallContext);

        // Assert
        Assert.NotNull(resultResponse);
    }

    [Fact]
    public async Task Test_GetFlowPorts()
    {
        // Arrange
        AsyncUnaryCall<GetFlowPortsResponse> mockCallGetFlowPorts = GrpcCallHelpers.CreateAsyncUnaryCall(new GetFlowPortsResponse());
        _mockClient.Setup(c => c.GetFlowPortsAsync(It.IsAny<GetFlowPortsRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(mockCallGetFlowPorts);
        var request = new GetFlowPortsRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        GetFlowPortsResponse resultResponse = await _service.GetFlowPorts(request, _serverCallContext);

        // Assert
        Assert.NotNull(resultResponse);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, false)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_AddFlowStep(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<AddFlowStepResponse> mockCallAddFlowStep = GrpcCallHelpers.CreateAsyncUnaryCall(new AddFlowStepResponse());
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockClient.Setup(c => c.AddFlowStepAsync(It.IsAny<AddFlowStepRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallAddFlowStep);
        var request = new AddFlowStepRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        AddFlowStepResponse resultResponse = null!;
        if (isAllowed)
        {
            resultResponse = await _service.AddFlowStep(request, _serverCallContext);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.AddFlowStep(request, _serverCallContext));
            return;
        }

        // Assert
        Assert.NotNull(resultResponse);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, false)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_DeleteFlowStep(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<Empty> mockCallDeleteFlowStep = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockClient.Setup(c => c.DeleteFlowStepAsync(It.IsAny<DeleteFlowStepRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallDeleteFlowStep);
        var request = new DeleteFlowStepRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        Empty resultResponse = null!;
        if (isAllowed)
        {
            resultResponse = await _service.DeleteFlowStep(request, _serverCallContext);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.DeleteFlowStep(request, _serverCallContext));
            return;
        }

        // Assert
        Assert.NotNull(resultResponse);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, false)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_MoveFlowStep(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<Empty> mockCallMoveFlowStep = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockClient.Setup(c => c.MoveFlowStepAsync(It.IsAny<MoveFlowStepRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallMoveFlowStep);
        var request = new MoveFlowStepRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        Empty resultResponse = null!;
        if (isAllowed)
        {
            resultResponse = await _service.MoveFlowStep(request, _serverCallContext);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.MoveFlowStep(request, _serverCallContext));
            return;
        }

        // Assert
        Assert.NotNull(resultResponse);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, false)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_LinkFlowPorts(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<LinkFlowPortsResponse> mockCallLinkFlowPorts = GrpcCallHelpers.CreateAsyncUnaryCall(new LinkFlowPortsResponse());
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockClient.Setup(c => c.LinkFlowPortsAsync(It.IsAny<LinkFlowPortsRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallLinkFlowPorts);
        var request = new LinkFlowPortsRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        LinkFlowPortsResponse resultResponse = null!;
        if (isAllowed)
        {
            resultResponse = await _service.LinkFlowPorts(request, _serverCallContext);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.LinkFlowPorts(request, _serverCallContext));
            return;
        }

        // Assert
        Assert.NotNull(resultResponse);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, false)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_UpdateFlowPort(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<Empty> mockCallUpdateFlowPort = GrpcCallHelpers.CreateAsyncUnaryCall(new Empty());
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockClient.Setup(c => c.UpdateFlowPortAsync(It.IsAny<UpdateFlowPortRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallUpdateFlowPort);
        var request = new UpdateFlowPortRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        Empty resultResponse = null!;
        if (isAllowed)
        {
            resultResponse = await _service.UpdateFlowPort(request, _serverCallContext);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.UpdateFlowPort(request, _serverCallContext));
            return;
        }

        // Assert
        Assert.NotNull(resultResponse);
    }

    [Fact]
    public async Task Test_GetImageStream()
    {
        // Arrange
        var chunks = new List<ImageChunkDto> {
            new ImageChunkDto()
        };
        AsyncServerStreamingCall<ImageChunkDto> mockCallGetImageStream = GrpcCallHelpers.CreateAsyncServerStreamingCall(chunks);
        _mockClient.Setup(c => c.GetImageStream(It.IsAny<GetImageStreamRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(mockCallGetImageStream);
        var request = new GetImageStreamRequest
        {
            AgentUniqueName = "Test"
        };

        var mockServerStreamWriter = new Mock<IServerStreamWriter<ImageChunkDto>>();

        // Act
        await _service.GetImageStream(request, mockServerStreamWriter.Object, _serverCallContext);

        // Assert
        _mockClient.Verify(c => c.GetImageStream(It.IsAny<GetImageStreamRequest>(), null, null, It.IsAny<CancellationToken>()), Times.Once);
        mockServerStreamWriter.Verify(w => w.WriteAsync(It.IsAny<ImageChunkDto>()), Times.Once);
    }
}
