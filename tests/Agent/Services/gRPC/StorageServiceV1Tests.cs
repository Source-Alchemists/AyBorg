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
using AyBorg.Authorization;
using Moq;

namespace AyBorg.Agent.Tests.Services.gRPC;

public class StorageServiceV1Tests : BaseGrpcServiceTests<StorageServiceV1, Storage.StorageClient>
{
    private readonly Mock<IStorageService> _mockStorageService = new();

    public StorageServiceV1Tests()
    {
        _service = new StorageServiceV1(_mockStorageService.Object);
    }

    [Theory]
    [InlineData(Roles.Administrator, true)]
    [InlineData(Roles.Engineer, true)]
    [InlineData(Roles.Reviewer, true)]
    [InlineData(Roles.Auditor, true)]
    [InlineData("", false)]
    public async Task Test_GetDirectories(string userRole, bool isAllowed)
    {
        // Arrange
        _mockContextUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", userRole) });
        _mockStorageService.Setup(m => m.GetDirectories(It.IsAny<string>())).Returns(new List<string> { "/Test " });
        var request = new GetDirectoriesRequest
        {
            Path = "/"
        };

        // Act
        if (!isAllowed)
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetDirectories(request, _serverCallContext));
            return;
        }

        GetDirectoriesResponse response = await _service.GetDirectories(request, _serverCallContext);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Directories);
    }
}
