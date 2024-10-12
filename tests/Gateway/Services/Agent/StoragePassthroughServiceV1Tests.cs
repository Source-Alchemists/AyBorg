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

public class StoragePassthroughServiceV1Tests : BaseGrpcServiceTests<StoragePassthroughServiceV1, Storage.StorageClient>
{
    public StoragePassthroughServiceV1Tests()
    {
        _service = new StoragePassthroughServiceV1(_mockGrpcChannelService.Object);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, true)]
    [InlineData(Roles.Auditor, false)]
    public async Task Test_GetDirectories(string userRole, bool isAllowed)
    {
        // Arrange
        AsyncUnaryCall<GetDirectoriesResponse> mockCallActivateProject = GrpcCallHelpers.CreateAsyncUnaryCall(new GetDirectoriesResponse());
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockClient.Setup(c => c.GetDirectoriesAsync(It.IsAny<GetDirectoriesRequest>(), It.IsAny<Metadata>(), null, It.IsAny<CancellationToken>())).Returns(mockCallActivateProject);
        var request = new GetDirectoriesRequest
        {
            AgentUniqueName = "Test"
        };

        // Act
        GetDirectoriesResponse resultResponse = null!;
        if (isAllowed)
        {
            resultResponse = await _service.GetDirectories(request, _serverCallContext);
        }
        else
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetDirectories(request, _serverCallContext));
            return;
        }

        // Assert
        Assert.NotNull(resultResponse);
    }
}
