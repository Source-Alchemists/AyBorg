using AyBorg.Agent.Services;
using AyBorg.Data.Agent;
using AyBorg.SDK.Projects;
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
    public async ValueTask Test_GetSettingsRecordAsync()
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
    public async ValueTask Test_TryUpdateActiveProjectSettingsAsync(bool hasMeta, bool isActive)
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
