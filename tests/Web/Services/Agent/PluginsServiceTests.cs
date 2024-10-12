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
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Tests.Helpers;
using Grpc.Core;
using Moq;

namespace AyBorg.Web.Tests.Services.Agent;

public class PluginsServiceTests
{
    private readonly Mock<IRpcMapper> _mockRpcMapper = new();
    private readonly Mock<Editor.EditorClient> _mockEditorClient = new();
    private readonly PluginsService _service;

    public PluginsServiceTests()
    {
        _service = new PluginsService(_mockRpcMapper.Object, _mockEditorClient.Object);
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
        _mockRpcMapper.Setup(x => x.FromRpc(It.IsAny<StepDto>())).Returns(new StepModel());

        // Act
        IEnumerable<StepModel> actual = await _service.ReceiveStepsAsync("Test");

        // Assert
        Assert.Equal(2, actual.Count());
    }
}
