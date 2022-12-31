using AyBorg.Agent.Services;
using AyBorg.Database.Data;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Data.DAL;
using AyBorg.SDK.Data.Mapper;
using AyBorg.SDK.Projects;
using AyBorg.SDK.System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Agent.Tests;

#nullable disable

public sealed class ProjectManagementServiceTests : IDisposable
{
    private static readonly NullLogger<ProjectManagementService> s_projLogger = new();
    private static readonly NullLogger<IServiceConfiguration> s_serviceLogger = new();
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly DbContextOptions<ProjectContext> _contextOptions;
    private readonly IServiceConfiguration _serviceConfiguration;
    private readonly Mock<IEngineHost> _mockEngineHost = new();
    private readonly Mock<IRuntimeToStorageMapper> _mockRuntimeToStorageMapper = new();
    private readonly Mock<IRuntimeConverterService> _mockRuntimeConverterService = new();
    private readonly Mock<IDbContextFactory<ProjectContext>> _mockDbContextFactory;
    private readonly ProjectManagementService _service;
    private bool _disposed = false;

    public ProjectManagementServiceTests()
    {
        _connection = new Microsoft.Data.Sqlite.SqliteConnection("Filename=:memory:");
        _connection.Open();
        _contextOptions = new DbContextOptionsBuilder<ProjectContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new ProjectContext(_contextOptions);
        context.Database.EnsureCreated();

        var settings = new Dictionary<string, string> {
            {"AyBorg:Service:UniqueName", "TestAgent"}
        };

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
        _serviceConfiguration = new ServiceConfiguration(s_serviceLogger, configuration);

        _mockDbContextFactory = CreateContextFactoryMock();
        _service = new ProjectManagementService(s_projLogger,
                                                    _serviceConfiguration,
                                                    _mockDbContextFactory.Object,
                                                    _mockEngineHost.Object,
                                                    _mockRuntimeToStorageMapper.Object,
                                                    _mockRuntimeConverterService.Object);
    }

    [Fact]
    public async Task Test_CreateFirstProject()
    {
        // Arrange
        _mockEngineHost.Setup(x => x.TryActivateProjectAsync(It.IsAny<Project>())).ReturnsAsync(true);

        // Act
        ProjectRecord result = await _service.CreateAsync("test");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test", result.Meta.Name);
        Assert.True(result.Meta.IsActive);
    }

    [Theory]
    [InlineData(false, ProjectState.Draft)]
    [InlineData(true, ProjectState.Draft)]
    [InlineData(false, ProjectState.Ready)]
    [InlineData(true, ProjectState.Ready)]
    public async Task Test_DeleteProject(bool isActive, ProjectState state)
    {
        // Arrange
        ProjectRecord databaseProjectRecord = await CreateDatabaseProject("test", isActive, state);

        if (isActive)
        {
            _mockEngineHost.Setup(x => x.ActiveProject).Returns(new Project
            {
                Meta = new ProjectMeta
                {
                    Id = databaseProjectRecord.Meta.Id,
                    Name = databaseProjectRecord.Meta.Name
                }
            });
        }

        _mockEngineHost.Setup(x => x.TryDeactivateProjectAsync()).ReturnsAsync(true);

        // Act
        ProjectManagementResult result = await _service.TryDeleteAsync(databaseProjectRecord.Meta.Id);

        // Assert
        Assert.True(result.IsSuccessful);
        using var context = new ProjectContext(_contextOptions);
        Assert.Empty(context.AyBorgProjects);
    }

    [Fact]
    public async Task Test_SaveActiveProject()
    {
        // Arrange
        ProjectRecord newDatabaseProjectRecord = await CreateDatabaseProject("test", true, ProjectState.Draft);
        _mockRuntimeToStorageMapper.Setup(x => x.Map(It.IsAny<Project>())).Returns(newDatabaseProjectRecord with
        {
            DbId = Guid.Empty,
            Settings = newDatabaseProjectRecord.Settings with
            {
                DbId = Guid.Empty
            }
        });

        StepProxy stepProxy1 = CreateStepProxy("Step 1");
        StepProxy stepProxy2 = CreateStepProxy("Step 2");
        var runtimeProject = new Project
        {
            Meta = new ProjectMeta
            {
                Id = newDatabaseProjectRecord.Meta.Id,
                Name = newDatabaseProjectRecord.Meta.Name
            },
            Steps = new List<IStepProxy>
            {
                stepProxy1,
                stepProxy2
            }
        };

        _mockEngineHost.Setup(x => x.ActiveProject).Returns(runtimeProject);

        // Act
        ProjectManagementResult result = await _service.TrySaveActiveAsync();

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Null(result.Message);
        Assert.NotEqual(Guid.Empty, result.ProjectMetaDbId);
        Assert.NotNull(result.ProjectMetaDbId);
        using ProjectContext context = await _mockDbContextFactory.Object.CreateDbContextAsync();
        Assert.Equal(2, context.AyBorgProjectMetas.Count());
        Assert.Equal(2, context.AyBorgProjects.Count());
        ProjectMetaRecord resultProjectMetaRecord = context.AyBorgProjectMetas.Single(pm => pm.DbId.Equals(result.ProjectMetaDbId));
        ProjectRecord resultProjectRecord = CreateCompleteProjectRecordQuery(context).Single(x => x.DbId.Equals(resultProjectMetaRecord.ProjectRecordId));
        ProjectMetaRecord previousProjectMetaRecord = context.AyBorgProjectMetas.Single(pm => pm.DbId.Equals(newDatabaseProjectRecord.Meta.DbId));
        Assert.NotEqual(newDatabaseProjectRecord.Meta.DbId, resultProjectMetaRecord.DbId);
        Assert.NotEqual(newDatabaseProjectRecord.DbId, resultProjectRecord.DbId);
        Assert.Equal(newDatabaseProjectRecord.Meta.Name, resultProjectRecord.Meta.Name);
        Assert.Equal(newDatabaseProjectRecord.Meta.Id, resultProjectMetaRecord.Id);
        Assert.False(previousProjectMetaRecord.IsActive);
        Assert.True(resultProjectRecord.Meta.IsActive);
        Assert.Equal(ProjectState.Draft, resultProjectRecord.Meta.State);
    }

    [Theory]
    [InlineData(ProjectState.Draft, null, true, ProjectState.Review, "test_1.2.3")]
    [InlineData(ProjectState.Draft, null, false, ProjectState.Review, "test_1.2.3")]
    [InlineData(ProjectState.Review, "auditor", true, ProjectState.Ready, "test_1.2.3")]
    [InlineData(ProjectState.Review, "auditor", false, ProjectState.Ready, "test_1.2.3")]
    public async Task Test_SaveNewVersionOfProject(ProjectState projectState, string auditor,
                                                    bool expectedActive, ProjectState expectedProjectState, string expectedVersioName)
    {
        // Arrange
        ProjectRecord initialDbProject = await CreateDatabaseProject("test", expectedActive, projectState);

        // Act
        ProjectManagementResult result = await _service.TrySaveNewVersionAsync(initialDbProject.Meta.DbId, expectedProjectState, expectedVersioName, string.Empty, auditor);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Null(result.Message);
        Assert.NotEqual(Guid.Empty, result.ProjectMetaDbId);
        Assert.NotNull(result.ProjectMetaDbId);
        using ProjectContext context = await _mockDbContextFactory.Object.CreateDbContextAsync();
        if (projectState == ProjectState.Draft)
        {
            Assert.Single(context.AyBorgProjectMetas.Where(x => x.DbId.Equals(initialDbProject.Meta.DbId)));
            Assert.Single(context.AyBorgProjects.Where(x => x.DbId.Equals(initialDbProject.DbId)));
            Assert.Equal(1, context.AyBorgProjectMetas.Count());
            Assert.Equal(1, context.AyBorgProjects.Count());
        }
        ProjectMetaRecord resultProjectMeta = await context.AyBorgProjectMetas.FirstAsync(x => x.DbId.Equals(result.ProjectMetaDbId));
        Assert.Equal(initialDbProject.Meta.VersionIteration + 1, resultProjectMeta.VersionIteration);
        Assert.Equal(expectedVersioName, resultProjectMeta.VersionName);
        Assert.Equal(expectedProjectState, resultProjectMeta.State);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _connection.Dispose();
            _disposed = true;
        }
    }

    private async ValueTask<ProjectRecord> CreateDatabaseProject(string name, bool isActive, ProjectState state)
    {
        using var context = new ProjectContext(_contextOptions);
        var project = new ProjectRecord
        {
            Meta = new ProjectMetaRecord
            {
                Id = Guid.NewGuid(),
                Name = name,
                IsActive = isActive,
                State = state
            },
            Settings = new ProjectSettingsRecord { DbId = Guid.Empty }
        };
        context.AyBorgProjects.Add(project);
        context.AyBorgProjectMetas.Add(project.Meta);
        await context.SaveChangesAsync();

        return await context.AyBorgProjects.FirstOrDefaultAsync(x => x.Meta.Name == "test");
    }

    private static StepProxy CreateStepProxy(string name, int x = 0, int y = 0)
    {
        var stepBodyMock = new Mock<IStepBody>();
        stepBodyMock.Setup(x => x.DefaultName).Returns("TestStepBody");
        stepBodyMock.Setup(x => x.Ports).Returns(new List<IPort>
        {
            new StringPort("TestPort", PortDirection.Input, "test"),
            new NumericPort("Input 1", PortDirection.Input, 123),
            new NumericPort("Input 2", PortDirection.Input, 456),
            new NumericPort("Output", PortDirection.Output, 579)

        });

        var stepProxy = new StepProxy(stepBodyMock.Object, x, y)
        {
            Name = name
        };
        return stepProxy;
    }

    private static IQueryable<ProjectRecord> CreateCompleteProjectRecordQuery(ProjectContext context)
    {
        return context.AyBorgProjects.Include(x => x.Meta)
                                .Include(x => x.Steps)
                                .ThenInclude(x => x.MetaInfo)
                                .Include(x => x.Steps)
                                .ThenInclude(x => x.Ports);
    }

    private Mock<IDbContextFactory<ProjectContext>> CreateContextFactoryMock()
    {
        var contextFactoryMock = new Mock<IDbContextFactory<ProjectContext>>();
        contextFactoryMock.Setup(x => x.CreateDbContext()).Returns(() => new ProjectContext(_contextOptions));
        contextFactoryMock.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => new ProjectContext(_contextOptions));
        return contextFactoryMock;
    }
}
