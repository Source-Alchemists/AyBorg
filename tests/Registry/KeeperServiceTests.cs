using AyBorg.Registry.Services;
using AyBorg.SDK.Data.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using AyBorg.Registry.Mapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using AyBorg.Database.Data;
using AyBorg.SDK.System.Configuration;

namespace AyBorg.Registry.Tests;

public sealed class KeeperServiceTests : IDisposable
{
    private static readonly NullLogger<KeeperService> _logger = new();
    private readonly NullLogger<IRegistryConfiguration> _registryConfigurationLogger = new();
    private readonly IConfiguration _configuration;
    private readonly IRegistryConfiguration _registryConfiguration;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly DbContextOptions<RegistryContext> _contextOptions;
    private readonly IDalMapper _dalMapper;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeeperServiceTests"/> class.
    /// </summary>
    public KeeperServiceTests()
    {
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(
            initialData: new List<KeyValuePair<string, string>> { 
                new("Kestrel:Endpoints:Https:Url", "https://localhost:5001") 
            }!).Build();

        _registryConfiguration = new RegistryConfiguration(_registryConfigurationLogger, _configuration);
        _dalMapper = new DalMapper();

        _connection = new Microsoft.Data.Sqlite.SqliteConnection("Filename=:memory:");
        _connection.Open();
        _contextOptions = new DbContextOptionsBuilder<RegistryContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new RegistryContext(_contextOptions);
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task TestRegisterAsync()
    {
        // Arrange
        using var service = new KeeperService(_logger, _configuration, _registryConfiguration, _dalMapper, CreateContextFactoryMock().Object);
        var entry = new RegistryEntryDto { Name = "Test", Url = "https://myservice:7777" };

        // Act
        var result = await service.RegisterAsync(entry);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterAsync(entry));
    }

    [Fact]
    public async Task TestRegisterKnownServiceAgain()
    {
        // Arrange
        using var service = new KeeperService(_logger, _configuration, _registryConfiguration, _dalMapper, CreateContextFactoryMock().Object);
        var entry = new RegistryEntryDto { Name = "Test", Url = "https://myservice:7777" };

        var expectedResult = await service.RegisterAsync(entry);
        await service.UnregisterAsync(expectedResult);

        // Act
        var result = await service.RegisterAsync(entry);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task TestUnregisterAsync_success()
    {
        // Arrange
        using var service = new KeeperService(_logger, _configuration, _registryConfiguration, _dalMapper, CreateContextFactoryMock().Object);
        var entry = new RegistryEntryDto { Name = "Test", Url = "https://myservice:7777" };

        // Act
        var result1 = await service.RegisterAsync(entry);
        await service.UnregisterAsync(result1);
        var result2 = await service.RegisterAsync(entry);

        // Assert
        Assert.NotEqual(Guid.Empty, result2);
    }

    [Fact]
    public async Task TestUnregisterAsync_unknownService()
    {
        // Arrange
        using var service = new KeeperService(_logger, _configuration, _registryConfiguration, _dalMapper, CreateContextFactoryMock().Object);

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UnregisterAsync(Guid.Empty));
    }

    [Fact]
    public async Task TestGetAllServicRegistryEntriesAsync()
    {
        // Arrange
        using var service = new KeeperService(_logger, _configuration, _registryConfiguration, _dalMapper, CreateContextFactoryMock().Object);
        var entry1 = new RegistryEntryDto { Name = "Test", Url = "https://myservice:7777" };
        var entry2 = new RegistryEntryDto { Name = "Test2", Url = "https://myservice2:7777" };

        // Act
        await service.RegisterAsync(entry1);
        await service.RegisterAsync(entry2);
        var result = await service.GetAllRegistryEntriesAsync();

        // Assert
        Assert.Equal(3, result.Count());
        Assert.Contains(entry1, result);
        Assert.Contains(entry2, result);
    }

    [Fact]
    public async Task TestFindRegistryEntriesAsync()
    {
        // Arrange
        using var service = new KeeperService(_logger, _configuration, _registryConfiguration, _dalMapper, CreateContextFactoryMock().Object);
        var entry1 = new RegistryEntryDto { Name = "Test", Url = "https://myservice:7777" };
        var entry2 = new RegistryEntryDto { Name = "Test2", Url = "https://myservice2:7777" };

        // Act
        await service.RegisterAsync(entry1);
        await service.RegisterAsync(entry2);
        var result = await service.FindRegistryEntriesAsync("Test");

        // Assert
        Assert.Single(result);
        Assert.Contains(entry1, result);
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

    private Mock<IDbContextFactory<RegistryContext>> CreateContextFactoryMock()
    {
        var contextFactoryMock = new Mock<IDbContextFactory<RegistryContext>>();
        contextFactoryMock.Setup(x => x.CreateDbContext()).Returns(() => new RegistryContext(_contextOptions));
        contextFactoryMock.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => new RegistryContext(_contextOptions));
        return contextFactoryMock;
    }
}