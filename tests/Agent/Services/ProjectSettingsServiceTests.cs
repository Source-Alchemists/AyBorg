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

using AyBorg.Agent.Services;
using AyBorg.Data.Agent;
using AyBorg.Runtime.Projects;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Agent.Tests.Services;

public class ProjectSettingsServiceTests
{
    private static readonly NullLogger<ProjectSettingsService> s_logger = new();
    private readonly Mock<IProjectRepository> _mockProjectRepository = new();
    private readonly Mock<IProjectManagementService> _mockProjectManagementService = new();
    private readonly Mock<IEngineHost> _mockEngineHost = new();
    private readonly ProjectSettingsRecord _projectSettingsRecord = new();

    private readonly ProjectSettingsService _service;

    public ProjectSettingsServiceTests()
    {
        _mockProjectRepository.Setup(r => r.GetSettingAsync(It.IsAny<Guid>())).ReturnsAsync(_projectSettingsRecord);

        _service = new ProjectSettingsService(s_logger, _mockProjectRepository.Object, _mockProjectManagementService.Object, _mockEngineHost.Object);
    }

    [Fact]
    public async Task Test_GetSettingsRecordAsync()
    {
        // Act
        ProjectSettingsRecord result = await _service.GetSettingsRecordAsync(Guid.NewGuid());

        // Assert
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Test_TryUpdateActiveProjectSettingsAsync(bool hasMeta, bool isActive)
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        _mockProjectRepository.Setup(r => r.GetAllMetasAsync()).ReturnsAsync(new List<ProjectMetaRecord>{
            new ProjectMetaRecord(),
            new ProjectMetaRecord { DbId = expectedId }
        });
        _mockProjectManagementService.Setup(s => s.ActiveProjectId).Returns(isActive ? expectedId : Guid.NewGuid());
        var newProjectSettings = new ProjectSettings
        {
            IsForceResultCommunicationEnabled = true
        };

        // Act
        bool result = await _service.TryUpdateActiveProjectSettingsAsync(hasMeta ? expectedId : Guid.NewGuid(), newProjectSettings);

        // Assert
        Assert.Equal(hasMeta, result);
    }
}
