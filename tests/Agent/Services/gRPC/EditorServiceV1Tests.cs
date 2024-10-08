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
using AyBorg.Agent.Services;
using AyBorg.Agent.Services.gRPC;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication.gRPC;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ImageTorque;
using ImageTorque.Buffers;
using ImageTorque.Pixels;
using Moq;

namespace AyBorg.Agent.Tests.Services.gRPC;

public class EditorServiceV1Tests : BaseGrpcServiceTests<EditorServiceV1, Editor.EditorClient>
{
    private readonly Mock<IPluginsService> _mockPluginsService = new();
    private readonly Mock<IFlowService> _mockFlowService = new();
    private readonly Mock<ICacheService> _mockCacheService = new();
    private readonly Mock<IRuntimeMapper> _mockRuntimeMapper = new();
    private readonly Mock<IRpcMapper> _mockRpcMapper = new();

    public EditorServiceV1Tests()
    {
        _service = new EditorServiceV1(s_logger, _mockPluginsService.Object, _mockFlowService.Object, _mockCacheService.Object, _mockRuntimeMapper.Object, _mockRpcMapper.Object);
    }

    [Fact]
    public async Task Test_GetAvailableSteps()
    {
        // Arrange
        _mockPluginsService.Setup(p => p.Steps).Returns(new List<IStepProxy>
        {
            CreateStepProxyMock("S1", Guid.NewGuid()).Object,
            CreateStepProxyMock("S2", Guid.NewGuid()).Object
        });
        _mockRuntimeMapper.Setup(r => r.FromRuntime(It.IsAny<IStepProxy>(), false)).Returns(new Step());
        _mockRpcMapper.Setup(r => r.ToRpc(It.IsAny<Step>())).Returns(new StepDto());
        var request = new GetAvailableStepsRequest();

        // Act
        GetAvailableStepsResponse response = await _service.GetAvailableSteps(request, _serverCallContext);

        // Assert
        Assert.Equal(2, response.Steps.Count);
    }

    [Theory]
    [InlineData(0, false, true, false, false)]
    [InlineData(0, false, false, false, true)]
    [InlineData(1, false, false, false, false)]
    [InlineData(1, false, false, true, false)]
    public async Task Test_GetFlowSteps(int expectedStepsCount, bool hasEmptyIdStr, bool hasInvalidIterationId, bool hasEmptyIterationId, bool hasInvalidStepId)
    {
        // Arrange
        Guid iterationId = hasEmptyIterationId ? Guid.Empty : Guid.NewGuid();
        Guid stepId = Guid.NewGuid();
        string iterationIdStr = hasEmptyIdStr ? string.Empty : iterationId.ToString();
        iterationIdStr = hasInvalidIterationId ? "42" : iterationIdStr;
        string stepIdStr = hasInvalidStepId ? "42" : stepId.ToString();

        var request = new GetFlowStepsRequest
        {
            IterationId = iterationIdStr
        };
        request.StepIds.Add(stepIdStr);

        _mockFlowService.Setup(f => f.GetSteps()).Returns(new List<IStepProxy>{
            CreateStepProxyMock("P1", stepId).Object
        });
        _mockCacheService.Setup(c => c.GetOrCreateStepEntry(It.IsAny<Guid>(), It.IsAny<IStepProxy>())).Returns(new Step());
        _mockRpcMapper.Setup(r => r.ToRpc(It.IsAny<Step>())).Returns(new StepDto());

        // Act
        if (!hasEmptyIdStr && hasInvalidIterationId)
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.GetFlowSteps(request, _serverCallContext));
            return;
        }

        GetFlowStepsResponse response = await _service.GetFlowSteps(request, _serverCallContext);

        // Assert
        Assert.Equal(expectedStepsCount, response.Steps.Count);
    }

    [Theory]
    [InlineData(0, true, true)]
    [InlineData(1, true, false)]
    public async Task Test_GetFlowLinks(int expectedLinksCount, bool hasLinkId, bool hasInvalidLinkId)
    {
        // Arrange
        var link1 = new PortLink(CreatePortMock("P1", Guid.NewGuid()).Object, CreatePortMock("P2", Guid.NewGuid()).Object);
        var link2 = new PortLink(CreatePortMock("P1", Guid.NewGuid()).Object, CreatePortMock("P2", Guid.NewGuid()).Object);
        var request = new GetFlowLinksRequest();
        if (hasLinkId)
        {
            request.LinkIds.Add(hasInvalidLinkId ? "42" : link2.Id.ToString());
        }
        _mockFlowService.Setup(f => f.GetLinks()).Returns(new List<PortLink> {
            link1,
            link2
        });
        _mockRpcMapper.Setup(r => r.ToRpc(It.IsAny<PortLink>())).Returns(new LinkDto());

        // Act
        GetFlowLinksResponse response = await _service.GetFlowLinks(request, _serverCallContext);

        // Assert
        Assert.Equal(expectedLinksCount, response.Links.Count);
    }

    [Theory]
    [InlineData(0, false, true, false, false, false)]
    [InlineData(0, false, false, false, true, false)]
    [InlineData(0, false, false, false, false, true)]
    [InlineData(1, false, false, false, false, false)]
    [InlineData(1, false, false, true, false, false)]
    public async Task Test_GetFlowPorts(int expectedPortsCount, bool hasEmptyIdStr, bool hasInvalidIterationId, bool hasEmptyIterationId, bool hasInvalidPortId, bool hasWrongPortId)
    {
        // Arrange
        Guid iterationId = hasEmptyIterationId ? Guid.Empty : Guid.NewGuid();
        Guid portId = Guid.NewGuid();
        string iterationIdStr = hasEmptyIdStr ? string.Empty : iterationId.ToString();
        iterationIdStr = hasInvalidIterationId ? "42" : iterationIdStr;
        string portIdStr = hasInvalidPortId ? "42" : portId.ToString();

        var request = new GetFlowPortsRequest
        {
            IterationId = iterationIdStr
        };
        request.PortIds.Add(portIdStr);

        _mockFlowService.Setup(f => f.GetPort(It.IsAny<Guid>())).Returns(hasWrongPortId ? null! : CreatePortMock("P1", Guid.NewGuid()).Object);
        _mockCacheService.Setup(c => c.GetOrCreatePortEntry(It.IsAny<Guid>(), It.IsAny<IPort>())).Returns(new Port());
        _mockRuntimeMapper.Setup(r => r.FromRuntime(It.IsAny<IPort>())).Returns(new Port());
        _mockRpcMapper.Setup(r => r.ToRpc(It.IsAny<Port>())).Returns(new PortDto());

        // Act
        if (!hasEmptyIdStr && hasInvalidIterationId)
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.GetFlowPorts(request, _serverCallContext));
            return;
        }

        GetFlowPortsResponse response = await _service.GetFlowPorts(request, _serverCallContext);

        // Assert
        Assert.Equal(expectedPortsCount, response.Ports.Count);
    }

    [Theory]
    [InlineData(Roles.Administrator, true, false, false)]
    [InlineData(Roles.Engineer, true, false, false)]
    [InlineData(Roles.Reviewer, false, false, false)]
    [InlineData(Roles.Auditor, false, false, false)]
    [InlineData(Roles.Administrator, true, false, true)]
    [InlineData(Roles.Administrator, true, true, false)]
    public async Task Test_AddFlowStep(string userRole, bool isAllowed, bool hasInvalidStepId, bool hasWrongStepId)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        var request = new AddFlowStepRequest
        {
            StepId = hasInvalidStepId ? "42" : Guid.NewGuid().ToString()
        };

        _mockFlowService.Setup(f => f.AddStepAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(hasWrongStepId ? null! : CreateStepProxyMock("P1", Guid.NewGuid()).Object);
        _mockRuntimeMapper.Setup(r => r.FromRuntime(It.IsAny<IStepProxy>(), false)).Returns(new Step());
        _mockRpcMapper.Setup(r => r.ToRpc(It.IsAny<Step>())).Returns(new StepDto());

        // Act
        if (!isAllowed)
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.AddFlowStep(request, _serverCallContext));
            return;
        }

        if (hasWrongStepId || hasInvalidStepId)
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.AddFlowStep(request, _serverCallContext));
            return;
        }

        AddFlowStepResponse response = await _service.AddFlowStep(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Step);
    }

    [Theory]
    [InlineData(Roles.Administrator, true, false, false)]
    [InlineData(Roles.Engineer, true, false, false)]
    [InlineData(Roles.Reviewer, false, false, false)]
    [InlineData(Roles.Auditor, false, false, false)]
    [InlineData(Roles.Administrator, true, false, true)]
    [InlineData(Roles.Administrator, true, true, false)]
    public async Task Test_DeleteFlowStep(string userRole, bool isAllowed, bool hasInvalidStepId, bool hasWrongStepId)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        var request = new DeleteFlowStepRequest
        {
            StepId = hasInvalidStepId ? "42" : Guid.NewGuid().ToString()
        };

        _mockFlowService.Setup(f => f.TryRemoveStepAsync(It.IsAny<Guid>())).ReturnsAsync(!hasWrongStepId);

        // Act
        if (!isAllowed)
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.DeleteFlowStep(request, _serverCallContext));
            return;
        }

        if (hasWrongStepId || hasInvalidStepId)
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.DeleteFlowStep(request, _serverCallContext));
            return;
        }

        Empty response = await _service.DeleteFlowStep(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
    }

    [Theory]
    [InlineData(Roles.Administrator, true, false, false)]
    [InlineData(Roles.Engineer, true, false, false)]
    [InlineData(Roles.Reviewer, false, false, false)]
    [InlineData(Roles.Auditor, false, false, false)]
    [InlineData(Roles.Administrator, true, false, true)]
    [InlineData(Roles.Administrator, true, true, false)]
    public async Task Test_MoveFlowStep(string userRole, bool isAllowed, bool hasInvalidStepId, bool hasWrongStepId)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        var request = new MoveFlowStepRequest
        {
            StepId = hasInvalidStepId ? "42" : Guid.NewGuid().ToString()
        };

        _mockFlowService.Setup(f => f.TryMoveStepAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(!hasWrongStepId);

        // Act
        if (!isAllowed)
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.MoveFlowStep(request, _serverCallContext));
            return;
        }

        if (hasWrongStepId || hasInvalidStepId)
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.MoveFlowStep(request, _serverCallContext));
            return;
        }

        Empty response = await _service.MoveFlowStep(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
    }

    [Theory]
    [InlineData(Roles.Administrator, true, false, false, false, false)]
    [InlineData(Roles.Engineer, true, false, false, false, false)]
    [InlineData(Roles.Reviewer, false, false, false, false, false)]
    [InlineData(Roles.Auditor, false, false, false, false, false)]
    [InlineData(Roles.Administrator, true, false, false, true, false)]
    [InlineData(Roles.Administrator, true, false, false, true, true)]
    [InlineData(Roles.Administrator, true, false, true, false, false)]
    [InlineData(Roles.Administrator, true, true, false, false, false)]
    public async Task Test_LinkFlowPorts(string userRole, bool isAllowed, bool hasInvalidSourceId, bool hasInvalidTargetId, bool isUnlinking, bool isUnlinkFailing)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        var request = new LinkFlowPortsRequest
        {
            SourceId = hasInvalidSourceId ? "42" : Guid.NewGuid().ToString(),
            TargetId = isUnlinking ? string.Empty : hasInvalidTargetId ? "42" : Guid.NewGuid().ToString()
        };

        _mockFlowService.Setup(f => f.LinkPortsAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(new PortLink(
                                                                                                        CreatePortMock("P1", Guid.NewGuid()).Object,
                                                                                                        CreatePortMock("P2", Guid.NewGuid()).Object));
        _mockFlowService.Setup(f => f.TryUnlinkPortsAsync(It.IsAny<Guid>())).ReturnsAsync(!isUnlinkFailing);

        // Act
        if (!isAllowed)
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.LinkFlowPorts(request, _serverCallContext));
            return;
        }

        if (isUnlinkFailing || hasInvalidSourceId || hasInvalidTargetId)
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.LinkFlowPorts(request, _serverCallContext));
            return;
        }

        LinkFlowPortsResponse response = await _service.LinkFlowPorts(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
        if (isUnlinking)
        {
            Assert.Equal(string.Empty, response.LinkId);
        }
        else
        {
            Assert.NotEqual(string.Empty, response.LinkId);
        }
    }

    [Theory]
    [InlineData(Roles.Administrator, true, false)]
    [InlineData(Roles.Engineer, true, false)]
    [InlineData(Roles.Reviewer, false, false)]
    [InlineData(Roles.Auditor, false, false)]
    [InlineData(Roles.Administrator, true, true)]
    public async Task Test_UpdateFlowPort(string userRole, bool isAllowed, bool isUpdateFailing)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockRpcMapper.Setup(r => r.FromRpc(It.IsAny<PortDto>())).Returns(new Port());
        _mockFlowService.Setup(f => f.TryUpdatePortValueAsync(It.IsAny<Guid>(), It.IsAny<object>())).ReturnsAsync(!isUpdateFailing);

        var request = new UpdateFlowPortRequest();

        // Act
        if (!isAllowed)
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.UpdateFlowPort(request, _serverCallContext));
            return;
        }

        if (isUpdateFailing)
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.UpdateFlowPort(request, _serverCallContext));
            return;
        }

        Empty response = await _service.UpdateFlowPort(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
    }

    [Theory]
    [InlineData(false, false, false, false, 100, 100)]
    [InlineData(false, false, false, false, 500, 500)]
    [InlineData(true, false, false, false, 100, 100)]
    [InlineData(false, true, false, false, 100, 100)]
    [InlineData(false, false, true, false, 100, 100)]
    [InlineData(false, false, false, true, 100, 100)]
    public async Task Test_GetImageStream(bool hasEmpyIterationId, bool hasInvalidIterationId, bool hasInvalidPortId, bool hasWrongPortId, int imageWidth, int imageHeight)
    {
        // Arrange
        var request = new GetImageStreamRequest
        {
            IterationId = hasEmpyIterationId ? Guid.Empty.ToString() : hasInvalidIterationId ? "42" : Guid.NewGuid().ToString(),
            PortId = hasInvalidPortId ? "42" : Guid.NewGuid().ToString()
        };

        var planarPixelBuffer = new PixelBuffer<L8>(imageWidth, imageHeight);
        var cachedImage = new CacheImage
        {
            OriginalImage = new Image(planarPixelBuffer)
        };

        _mockFlowService.Setup(f => f.GetPort(It.IsAny<Guid>())).Returns(hasWrongPortId ? null! : CreatePortMock("P1", Guid.NewGuid()).Object);
        _mockCacheService.Setup(c => c.GetOrCreatePortEntry(It.IsAny<Guid>(), It.IsAny<IPort>())).Returns(new Port { Value = cachedImage });
        _mockRuntimeMapper.Setup(r => r.FromRuntime(It.IsAny<IPort>())).Returns(new Port { Value = cachedImage });

        var mockServerStreamWriter = new Mock<IServerStreamWriter<ImageChunkDto>>();

        // Act
        if (hasInvalidIterationId || hasInvalidPortId || hasWrongPortId)
        {
            await Assert.ThrowsAsync<RpcException>(() => _service.GetImageStream(request, mockServerStreamWriter.Object, _serverCallContext));
            return;
        }

        await _service.GetImageStream(request, mockServerStreamWriter.Object, _serverCallContext);

        // Assert
        mockServerStreamWriter.Verify(w => w.WriteAsync(It.IsAny<ImageChunkDto>(), It.IsAny<CancellationToken>()));
    }

    private static Mock<IStepProxy> CreateStepProxyMock(string name, Guid id)
    {
        var stepProxyMock = new Mock<IStepProxy>();
        stepProxyMock.Setup(s => s.Name).Returns(name);
        stepProxyMock.Setup(s => s.Id).Returns(id);
        return stepProxyMock;
    }

    private static Mock<IPort> CreatePortMock(string name, Guid id)
    {
        var portMock = new Mock<IPort>();
        portMock.Setup(p => p.Name).Returns(name);
        portMock.Setup(p => p.Id).Returns(id);
        return portMock;
    }
}
