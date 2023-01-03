using AyBorg.Agent.Services;
using AyBorg.Database.Data;
using AyBorg.SDK.Data.DAL;
using AyBorg.SDK.Data.Mapper;
using AyBorg.SDK.Projects;
using AyBorg.SDK.System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Agent.Tests;

public sealed class ProjectManagementServiceTests
{
    private const string TestServiceUniqueName = "AyBorg.Agent";
    private static readonly NullLogger<ProjectManagementService> s_projLogger = new();
    private static readonly NullLogger<IServiceConfiguration> s_serviceLogger = new();
    private readonly IServiceConfiguration _serviceConfiguration;
    private readonly Mock<IEngineHost> _mockEngineHost = new();
    private readonly Mock<IRuntimeToStorageMapper> _mockRuntimeToStorageMapper = new();
    private readonly Mock<IRuntimeConverterService> _mockRuntimeConverterService = new();
    private readonly Mock<IProjectRepository> _mockProjectRepository = new();
    private readonly ProjectManagementService _service;

    public ProjectManagementServiceTests()
    {
        var settings = new Dictionary<string, string> {
            {"AyBorg:Service:UniqueName", TestServiceUniqueName}
        };

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings!)
            .Build();
        _serviceConfiguration = new ServiceConfiguration(s_serviceLogger, configuration);

        _service = new ProjectManagementService(s_projLogger,
                                                    _serviceConfiguration,
                                                    _mockProjectRepository.Object,
                                                    _mockEngineHost.Object,
                                                    _mockRuntimeToStorageMapper.Object,
                                                    _mockRuntimeConverterService.Object);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async ValueTask Test_CreateAsync(bool hasActiveProject, bool isActivateSuccessful)
    {
        // Arrange
        var expectedProject = new ProjectRecord
        {
            Meta = new ProjectMetaRecord
            {
                Name = "Test_Project",
                ServiceUniqueName = TestServiceUniqueName
            }
        };
        _mockProjectRepository.Setup(r => r.CreateAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(expectedProject);
        _mockProjectRepository.Setup(r => r.FindAsync(It.IsAny<Guid>())).ReturnsAsync(expectedProject);
        _mockProjectRepository.Setup(r => r.FindMetaAsync(It.IsAny<Guid>())).ReturnsAsync(expectedProject.Meta);
        _mockProjectRepository.Setup(r => r.GetAllMetasAsync(It.IsAny<string>())).ReturnsAsync(new List<ProjectMetaRecord> { new ProjectMetaRecord { IsActive = hasActiveProject } });
        _mockProjectRepository.Setup(r => r.TryUpdateAsync(It.IsAny<ProjectMetaRecord>())).ReturnsAsync(isActivateSuccessful);
        _mockEngineHost.Setup(e => e.TryActivateProjectAsync(It.IsAny<Project>())).ReturnsAsync(true);

        // Act
        ProjectRecord result = await _service.CreateAsync("Test_Project");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(!hasActiveProject, result.Meta.IsActive);
    }

    [Theory]
    [InlineData(true, true, true, true)]
    [InlineData(false, true, true, false)]
    [InlineData(false, true, false, true)]
    [InlineData(false, false, true, true)]
    public async ValueTask Test_TryDeleteAsync(bool expectedSuccess, bool isActiveProject, bool isDeactivationSuccessful, bool isDeleteSuccessful)
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _mockProjectRepository.Setup(r => r.GetAllMetasAsync(It.IsAny<Guid>())).ReturnsAsync(new List<ProjectMetaRecord> { new ProjectMetaRecord { Id = projectId, IsActive = isActiveProject } });
        _mockProjectRepository.Setup(r => r.TryDeleteAsync(It.IsAny<Guid>())).ReturnsAsync(isDeleteSuccessful);
        _mockEngineHost.Setup(e => e.ActiveProject).Returns(new Project
        {
            Meta = new ProjectMeta
            {
                Id = isActiveProject ? projectId : Guid.NewGuid()
            }
        });
        _mockEngineHost.Setup(e => e.TryDeactivateProjectAsync()).ReturnsAsync(isDeactivationSuccessful);

        // Act
        ProjectManagementResult result = await _service.TryDeleteAsync(projectId);

        // Assert
        Assert.Equal(expectedSuccess, result.IsSuccessful);
    }

    [Theory]
    [InlineData(true, true, true, true, true, true, true, true)]
    [InlineData(false, true, true, false, true, true, true, true)]
    [InlineData(false, true, true, true, false, true, true, true)]
    [InlineData(false, true, true, true, true, false, true, true)]
    [InlineData(false, true, true, true, true, true, false, true)]
    [InlineData(false, true, true, true, true, true, true, false)]
    public async ValueTask Test_TryChangeActivationStateAsync(bool expectedSuccess, bool activate, bool hasActiveProject, bool containsProject, bool isValidService, bool isDeactivationSuccessful, bool isUpdateSuccessful, bool isActivateSuccessful)
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _mockProjectRepository.Setup(r => r.FindAsync(It.IsAny<Guid>())).ReturnsAsync(new ProjectRecord());
        _mockProjectRepository.Setup(r => r.FindMetaAsync(It.IsAny<Guid>())).ReturnsAsync(containsProject ? new ProjectMetaRecord
        {
            Id = projectId,
            ServiceUniqueName = isValidService ? TestServiceUniqueName : "Test123"
        } : null!);
        _mockProjectRepository.Setup(r => r.GetAllMetasAsync(It.IsAny<string>())).ReturnsAsync(new List<ProjectMetaRecord> { new ProjectMetaRecord { IsActive = hasActiveProject } });
        _mockProjectRepository.Setup(r => r.TryUpdateAsync(It.IsAny<ProjectMetaRecord>())).ReturnsAsync(isUpdateSuccessful);
        _mockEngineHost.Setup(e => e.TryDeactivateProjectAsync()).ReturnsAsync(isDeactivationSuccessful);
        _mockEngineHost.Setup(e => e.TryActivateProjectAsync(It.IsAny<Project>())).ReturnsAsync(isActivateSuccessful);
        _mockRuntimeConverterService.Setup(r => r.ConvertAsync(It.IsAny<ProjectRecord>())).ReturnsAsync(new Project());

        // Act
        ProjectManagementResult result = await _service.TryChangeActivationStateAsync(projectId, activate);

        // Assert
        Assert.Equal(expectedSuccess, result.IsSuccessful);
    }

    [Theory]
    [InlineData(true, true, true, true)]
    [InlineData(false, true, true, false)]
    [InlineData(false, true, false, true)]
    [InlineData(false, false, true, true)]
    public async ValueTask Test_TryLoadActiveAsync(bool expectedSuccess, bool hasActiveProject, bool isUpdateSuccessful, bool isChangeActivationSuccessful)
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _mockProjectRepository.Setup(r => r.GetAllMetasAsync(It.IsAny<string>())).ReturnsAsync(new List<ProjectMetaRecord> { new ProjectMetaRecord { IsActive = hasActiveProject } });
        _mockProjectRepository.Setup(r => r.TryUpdateAsync(It.IsAny<ProjectMetaRecord>())).ReturnsAsync(isUpdateSuccessful);
        _mockProjectRepository.Setup(r => r.FindMetaAsync(It.IsAny<Guid>())).ReturnsAsync(isChangeActivationSuccessful ? new ProjectMetaRecord
        {
            Id = projectId,
            ServiceUniqueName = TestServiceUniqueName
        } : null!);
        _mockProjectRepository.Setup(r => r.FindAsync(It.IsAny<Guid>())).ReturnsAsync(new ProjectRecord());
        _mockEngineHost.Setup(e => e.TryActivateProjectAsync(It.IsAny<Project>())).ReturnsAsync(isChangeActivationSuccessful);
        _mockEngineHost.Setup(e => e.TryDeactivateProjectAsync()).ReturnsAsync(isChangeActivationSuccessful);
        _mockRuntimeConverterService.Setup(r => r.ConvertAsync(It.IsAny<ProjectRecord>())).ReturnsAsync(new Project());

        // Act
        ProjectManagementResult result = await _service.TryLoadActiveAsync();

        // Assert
        Assert.Equal(expectedSuccess, result.IsSuccessful);
    }

    [Theory]
    [InlineData(true, true, true, true, true)]
    [InlineData(false, true, true, true, false)]
    [InlineData(false, true, true, false, true)]
    [InlineData(false, true, false, true, true)]
    [InlineData(false, false, true, true, true)]
    public async ValueTask Test_TrySaveActiveAsync(bool expectedSuccess, bool hasEngineActiveProject, bool hasActiveProject, bool isUpdateSuccessful, bool isSaveSuccessful)
    {
        // Arrange
        _mockEngineHost.Setup(e => e.ActiveProject).Returns(hasEngineActiveProject ? new Project() : null!);
        _mockProjectRepository.Setup(r => r.GetAllMetasAsync(It.IsAny<string>())).ReturnsAsync(new List<ProjectMetaRecord> { new ProjectMetaRecord { IsActive = hasActiveProject } });
        _mockProjectRepository.Setup(r => r.TryUpdateAsync(It.IsAny<ProjectMetaRecord>())).ReturnsAsync(isUpdateSuccessful);
        _mockProjectRepository.Setup(r => r.TrySave(It.IsAny<ProjectRecord>())).ReturnsAsync(isSaveSuccessful);
        _mockRuntimeToStorageMapper.Setup(r => r.Map(It.IsAny<Project>())).Returns(new ProjectRecord());

        // Act
        ProjectManagementResult result = await _service.TrySaveActiveAsync();

        // Assert
        Assert.Equal(expectedSuccess, result.IsSuccessful);
    }

    [Theory]
    [InlineData(true, ProjectState.Draft, ProjectState.Draft, null, true, true, true)]
    [InlineData(false, ProjectState.Draft, ProjectState.Draft, null, true, true, false)]
    [InlineData(false, ProjectState.Draft, ProjectState.Draft, null, true, false, true)]
    [InlineData(true, ProjectState.Review, ProjectState.Draft, null, true, true, true)]
    [InlineData(false, ProjectState.Review, ProjectState.Draft, null, true, false, true)]
    [InlineData(true, ProjectState.Review, ProjectState.Ready, "Test_approver", true, true, true)]
    [InlineData(false, ProjectState.Review, ProjectState.Ready, null, true, true, true)]
    public async ValueTask Test_TrySaveNewVersionAsync(bool expectedSuccess, ProjectState oldProjectState, ProjectState newProjectState, string? approver, bool containsProject, bool isUpdateSuccessful, bool isRemoveSuccessful)
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _mockProjectRepository.Setup(r => r.FindAsync(It.IsAny<Guid>())).ReturnsAsync(new ProjectRecord
        {
            Steps = new List<StepRecord> { new StepRecord {
                Ports = new List<PortRecord> { new PortRecord() }
            } },
            Links = new List<LinkRecord> { new LinkRecord() }
        });
        _mockProjectRepository.Setup(r => r.FindMetaAsync(It.IsAny<Guid>())).ReturnsAsync(containsProject ? new ProjectMetaRecord
        {
            Id = projectId,
            ServiceUniqueName = TestServiceUniqueName,
            State = oldProjectState
        } : null!);
        _mockProjectRepository.Setup(r => r.TryUpdateAsync(It.IsAny<ProjectMetaRecord>())).ReturnsAsync(isUpdateSuccessful);
        _mockProjectRepository.Setup(r => r.TryRemoveRangeAsync(It.IsAny<IEnumerable<ProjectMetaRecord>>())).ReturnsAsync(isRemoveSuccessful);

        // Act
        ProjectManagementResult result = await _service.TrySaveNewVersionAsync(projectId, newProjectState, "1.2.3", "Test_comment", approver);

        // Assert
        Assert.Equal(expectedSuccess, result.IsSuccessful);
    }
}
