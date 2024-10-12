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

using AyBorg.Agent.Guards;
using AyBorg.Agent.Services;
using AyBorg.Data.Agent;
using AyBorg.Runtime.Projects;
using Microsoft.AspNetCore.Http;
using Moq;

namespace AyBorg.Agent.Tests.Guards;

public class ProjectStateGuardMiddlewareTests
{
    private readonly Mock<IProjectManagementService> _projectManagementServiceMock;
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly ProjectStateGuardMiddleware _middleware;

    public ProjectStateGuardMiddlewareTests()
    {
        _projectManagementServiceMock = new Mock<IProjectManagementService>();
        _nextMock = new Mock<RequestDelegate>();
        _middleware = new ProjectStateGuardMiddleware(_nextMock.Object, _projectManagementServiceMock.Object);
    }

    [Theory]
    [InlineData("GET", "/", ProjectState.Draft)]
    [InlineData("GET", "/", ProjectState.Review)]
    [InlineData("GET", "/", ProjectState.Ready)]
    [InlineData("POST", "/", ProjectState.Draft)]
    [InlineData("POST", "/", ProjectState.Review)]
    [InlineData("POST", "/", ProjectState.Ready)]
    [InlineData("GET", "/projects", ProjectState.Draft)]
    [InlineData("GET", "/projects", ProjectState.Review)]
    [InlineData("GET", "/projects", ProjectState.Ready)]
    [InlineData("POST", "/projects", ProjectState.Draft)]
    [InlineData("POST", "/projects", ProjectState.Review)]
    [InlineData("POST", "/projects", ProjectState.Ready)]
    [InlineData("GET", "/flow", ProjectState.Draft)]
    [InlineData("GET", "/flow", ProjectState.Review)]
    [InlineData("GET", "/flow", ProjectState.Ready)]
    [InlineData("POST", "/flow", ProjectState.Draft)]
    [InlineData("POST", "/flow", ProjectState.Review)]
    [InlineData("POST", "/flow", ProjectState.Ready)]
    [InlineData("PUT", "/flow", ProjectState.Draft)]
    [InlineData("PUT", "/flow", ProjectState.Review)]
    [InlineData("PUT", "/flow", ProjectState.Ready)]
    [InlineData("DELETE", "/flow", ProjectState.Draft)]
    [InlineData("DELETE", "/flow", ProjectState.Review)]
    [InlineData("DELETE", "/flow", ProjectState.Ready)]
    public async Task Test_InvokeAsync(string method, string path, ProjectState projectState)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;

        _projectManagementServiceMock.SetupGet(service => service.ActiveProjectId).Returns(Guid.NewGuid());
        _projectManagementServiceMock.Setup(service => service.GetAllMetasAsync()).ReturnsAsync(new List<ProjectMetaRecord>
        {
            new ProjectMetaRecord
            {
                Id = _projectManagementServiceMock.Object.ActiveProjectId,
                State = projectState
            }
        });

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        if (method != "GET" && path.StartsWith("/flow") && projectState != ProjectState.Draft)
        {
            _nextMock.Verify(next => next(context), Times.Never);
            Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        }
        else
        {
            _nextMock.Verify(next => next(context), Times.Once);
        }
    }

    [Fact]
    public async Task Test_InvokeAsync_NoActiveProject()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/flow";

        _projectManagementServiceMock.SetupGet(service => service.ActiveProjectId).Returns(Guid.Empty);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Never);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task Test_InvokeAsync_NoProjectMeta()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/flow";

        _projectManagementServiceMock.SetupGet(service => service.ActiveProjectId).Returns(Guid.NewGuid());
        _projectManagementServiceMock.Setup(service => service.GetAllMetasAsync()).ReturnsAsync(new List<ProjectMetaRecord>());

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Never);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }
}
