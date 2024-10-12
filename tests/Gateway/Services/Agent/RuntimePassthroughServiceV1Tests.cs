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
using AyBorg.Gateway.Services.Tests;
using AyBorg.Gateway.Tests.Helpers;
using AyBorg.Authorization;

using Grpc.Core;

using Moq;

namespace AyBorg.Gateway.Services.Agent.Tests;

public class RuntimePassthroughServiceV1Tests : BaseGrpcServiceTests<RuntimePassthroughServiceV1, Ayborg.Gateway.Agent.V1.Runtime.RuntimeClient>
{
    public RuntimePassthroughServiceV1Tests()
    {
        _service = new RuntimePassthroughServiceV1(_mockGrpcChannelService.Object);
    }

    [Fact]
    public async Task Test_GetStatus()
    {
        // Arrange
        AsyncUnaryCall<GetRuntimeStatusResponse> mockCallGetAvailableSteps = GrpcCallHelpers.CreateAsyncUnaryCall(new GetRuntimeStatusResponse());
        _mockClient.Setup(c => c.GetStatusAsync(It.IsAny<GetRuntimeStatusRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(mockCallGetAvailableSteps);
        var request = new GetRuntimeStatusRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        GetRuntimeStatusResponse resultResponse = await _service.GetStatus(request, _serverCallContext);

        // Assert
        Assert.NotNull(resultResponse);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, true)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_StartRun(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<StartRunResponse> mockCallActivateProject = GrpcCallHelpers.CreateAsyncUnaryCall(new StartRunResponse());
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockClient.Setup(c => c.StartRunAsync(It.IsAny<StartRunRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallActivateProject);
        var request = new StartRunRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        StartRunResponse resultResponse = null!;
        if (isAllowed)
        {
            resultResponse = await _service.StartRun(request, _serverCallContext);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.StartRun(request, _serverCallContext));
            return;
        }

        // Assert
        Assert.NotNull(resultResponse);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, true)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_StopRun(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<StopRunResponse> mockCallActivateProject = GrpcCallHelpers.CreateAsyncUnaryCall(new StopRunResponse());
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockClient.Setup(c => c.StopRunAsync(It.IsAny<StopRunRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallActivateProject);
        var request = new StopRunRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        StopRunResponse resultResponse = null!;
        if (isAllowed)
        {
            resultResponse = await _service.StopRun(request, _serverCallContext);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.StopRun(request, _serverCallContext));
            return;
        }

        // Assert
        Assert.NotNull(resultResponse);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, true)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_AbortRun(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<AbortRunResponse> mockCallActivateProject = GrpcCallHelpers.CreateAsyncUnaryCall(new AbortRunResponse());
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockClient.Setup(c => c.AbortRunAsync(It.IsAny<AbortRunRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallActivateProject);
        var request = new AbortRunRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        AbortRunResponse resultResponse = null!;
        if (isAllowed)
        {
            resultResponse = await _service.AbortRun(request, _serverCallContext);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.AbortRun(request, _serverCallContext));
            return;
        }

        // Assert
        Assert.NotNull(resultResponse);
    }
}
