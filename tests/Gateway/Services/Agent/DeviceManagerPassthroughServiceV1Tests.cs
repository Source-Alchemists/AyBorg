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
using Grpc.Core;
using Moq;

namespace AyBorg.Gateway.Services.Agent.Tests;

public class DeviceManagerPassthroughServiceV1Tests : BaseGrpcServiceTests<DeviceManagerPassthroughServiceV1, DeviceManager.DeviceManagerClient>
{
    public DeviceManagerPassthroughServiceV1Tests()
    {
        _service = new DeviceManagerPassthroughServiceV1(_mockGrpcChannelService.Object);
    }

    [Fact]
    public async Task Test_GetAvailableProviders()
    {
        // Arrange
        AsyncUnaryCall<DeviceProviderCollectionResponse> call = GrpcCallHelpers.CreateAsyncUnaryCall(new DeviceProviderCollectionResponse());
        _mockClient.Setup(m => m.GetAvailableProvidersAsync(It.IsAny<DefaultAgentRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        // Act
        DeviceProviderCollectionResponse response = await _service.GetAvailableProviders(new DefaultAgentRequest
        {
            AgentUniqueName = "TestAgent"
        }, _serverCallContext);

        // Assert
        Assert.NotNull(response);
    }

    [Fact]
    public async Task Test_GetDevice()
    {
        // Arrange
        AsyncUnaryCall<DeviceDto> call = GrpcCallHelpers.CreateAsyncUnaryCall(new DeviceDto());
        _mockClient.Setup(m => m.GetDeviceAsync(It.IsAny<GetDeviceRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(call);

        // Act
        DeviceDto response = await _service.GetDevice(new GetDeviceRequest
        {
            AgentUniqueName = "TestAgent",
            DeviceId = "123"
        }, _serverCallContext);

        // Assert
        Assert.NotNull(response);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Auditor, false)]
    [InlineData(Roles.Reviewer, false)]
    public async Task Test_Add(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<DeviceDto> call = GrpcCallHelpers.CreateAsyncUnaryCall(new DeviceDto());
        _mockContextUser.Setup(m => m.Claims).Returns(new List<Claim> {
            new Claim("role", userRole)
        });
        _mockClient.Setup(m => m.AddAsync(It.IsAny<AddDeviceRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(call);

        var request = new AddDeviceRequest
        {
            AgentUniqueName = "TestAgent",
            DeviceProviderName = "TestProvider",
            DeviceId = "123",
            DevicePrefix = "TestPrefix"
        };

        // Act
        DeviceDto response = null!;
        if (isAllowed)
        {
            response = await _service.Add(request, _serverCallContext);

            Assert.NotNull(response);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.Add(request, _serverCallContext));
        }
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Auditor, false)]
    [InlineData(Roles.Reviewer, false)]
    public async Task Test_Remove(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<DeviceDto> call = GrpcCallHelpers.CreateAsyncUnaryCall(new DeviceDto());
        _mockContextUser.Setup(m => m.Claims).Returns(new List<Claim> {
            new Claim("role", userRole)
        });
        _mockClient.Setup(m => m.RemoveAsync(It.IsAny<RemoveDeviceRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(call);

        var request = new RemoveDeviceRequest
        {
            AgentUniqueName = "TestAgent",
            DeviceId = "123"
        };

        // Act
        DeviceDto response = null!;
        if (isAllowed)
        {
            response = await _service.Remove(request, _serverCallContext);

            Assert.NotNull(response);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.Remove(request, _serverCallContext));
        }
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Auditor, false)]
    [InlineData(Roles.Reviewer, false)]
    public async Task Test_ChangeState(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<DeviceDto> call = GrpcCallHelpers.CreateAsyncUnaryCall(new DeviceDto());
        _mockContextUser.Setup(m => m.Claims).Returns(new List<Claim> {
            new Claim("role", userRole)
        });
        _mockClient.Setup(m => m.ChangeStateAsync(It.IsAny<DeviceStateRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(call);

        var request = new DeviceStateRequest
        {
            AgentUniqueName = "TestAgent",
            DeviceId = "123"
        };

        // Act
        DeviceDto response = null!;
        if (isAllowed)
        {
            response = await _service.ChangeState(request, _serverCallContext);

            Assert.NotNull(response);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.ChangeState(request, _serverCallContext));
        }
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Auditor, false)]
    [InlineData(Roles.Reviewer, false)]
    public async Task Test_UpdateDevice(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<DeviceDto> call = GrpcCallHelpers.CreateAsyncUnaryCall(new DeviceDto());
        _mockContextUser.Setup(m => m.Claims).Returns(new List<Claim> {
            new Claim("role", userRole)
        });
        _mockClient.Setup(m => m.UpdateDeviceAsync(It.IsAny<UpdateDeviceRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(call);

        var request = new UpdateDeviceRequest
        {
            AgentUniqueName = "TestAgent",
            DeviceId = "123"
        };

        // Act
        DeviceDto response = null!;
        if (isAllowed)
        {
            response = await _service.UpdateDevice(request, _serverCallContext);

            Assert.NotNull(response);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.UpdateDevice(request, _serverCallContext));
        }
    }
}
