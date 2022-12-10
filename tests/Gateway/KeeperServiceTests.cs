using AyBorg.Database.Data;
using AyBorg.Gateway.Mapper;
using AyBorg.Gateway.Services;
using AyBorg.SDK.Data.DTOs;
using AyBorg.SDK.System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Gateway.Tests;

public sealed class KeeperServiceTests : IDisposable
{
    private static readonly NullLogger<KeeperService> s_logger = new();
    private readonly NullLogger<IGatewayConfiguration> _registryConfigurationLogger = new();
    private readonly IConfiguration _configuration;
    private readonly IGatewayConfiguration _registryConfiguration;
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

        _registryConfiguration = new GatewayConfiguration(_registryConfigurationLogger, _configuration);
        _dalMapper = new DalMapper();

        _connection = new Microsoft.Data.Sqlite.SqliteConnection("Filename=:memory:");
        _connection.Open();
        _contextOptions = new DbContextOptionsBuilder<RegistryContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new RegistryContext(_contextOptions);
        context.Database.EnsureCreated();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("http://test.org", "")]
    [InlineData("", "https://test.org")]
    public void TestConfigurationServerUrl(string httpUrl, string httpsUrl)
    {
        // Arrange
        IConfigurationRoot configuration = new ConfigurationBuilder().AddInMemoryCollection(
            initialData: new List<KeyValuePair<string, string>> {
                new("Kestrel:Endpoints:Http:Url", httpUrl),
                new("Kestrel:Endpoints:Https:Url", httpsUrl)
            }!).Build();
        var registryConfiguration = new GatewayConfiguration(_registryConfigurationLogger, configuration);

        // Act
        KeeperService service;
        if (string.IsNullOrEmpty(httpsUrl) && string.IsNullOrEmpty(httpUrl))
        {
            Assert.Throws<InvalidOperationException>(() => service = new KeeperService(s_logger, configuration, registryConfiguration, _dalMapper, CreateContextFactoryMock().Object));
        }
        else
        {
            service = new KeeperService(s_logger, configuration, registryConfiguration, _dalMapper, CreateContextFactoryMock().Object);

            // Assert
            Assert.NotNull(service);
        }
    }

    [Fact]
    public async Task TestRegisterAsync()
    {
        // Arrange
        using var service = new KeeperService(s_logger, _configuration, _registryConfiguration, _dalMapper, CreateContextFactoryMock().Object);
        var entry = new RegistryEntryDto { Name = "Test", Url = "https://myservice:7777" };

        // Act
        Guid result = await service.RegisterAsync(entry);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterAsync(entry));
    }

    [Fact]
    public async Task TestRegisterKnownServiceAgain()
    {
        // Arrange
        using var service = new KeeperService(s_logger, _configuration, _registryConfiguration, _dalMapper, CreateContextFactoryMock().Object);
        var entry = new RegistryEntryDto { Name = "Test", Url = "https://myservice:7777" };

        Guid expectedResult = await service.RegisterAsync(entry);
        await service.UnregisterAsync(expectedResult);

        // Act
        Guid result = await service.RegisterAsync(entry);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task TestUnregisterAsync_success()
    {
        // Arrange
        using var service = new KeeperService(s_logger, _configuration, _registryConfiguration, _dalMapper, CreateContextFactoryMock().Object);
        var entry = new RegistryEntryDto { Name = "Test", Url = "https://myservice:7777" };

        // Act
        Guid result1 = await service.RegisterAsync(entry);
        await service.UnregisterAsync(result1);
        Guid result2 = await service.RegisterAsync(entry);

        // Assert
        Assert.NotEqual(Guid.Empty, result2);
    }

    [Fact]
    public async Task TestUnregisterAsync_unknownService()
    {
        // Arrange
        using var service = new KeeperService(s_logger, _configuration, _registryConfiguration, _dalMapper, CreateContextFactoryMock().Object);

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UnregisterAsync(Guid.Empty));
    }

    [Fact]
    public async Task TestGetAllServicRegistryEntriesAsync()
    {
        // Arrange
        using var service = new KeeperService(s_logger, _configuration, _registryConfiguration, _dalMapper, CreateContextFactoryMock().Object);
        var entry1 = new RegistryEntryDto { Name = "Test", Url = "https://myservice:7777" };
        var entry2 = new RegistryEntryDto { Name = "Test2", Url = "https://myservice2:7777" };

        // Act
        await service.RegisterAsync(entry1);
        await service.RegisterAsync(entry2);
        IEnumerable<RegistryEntryDto> result = await service.GetAllRegistryEntriesAsync();

        // Assert
        Assert.Equal(3, result.Count());
        Assert.Contains(entry1, result);
        Assert.Contains(entry2, result);
    }

    [Fact]
    public async Task TestFindRegistryEntriesAsync()
    {
        // Arrange
        using var service = new KeeperService(s_logger, _configuration, _registryConfiguration, _dalMapper, CreateContextFactoryMock().Object);
        var entry1 = new RegistryEntryDto { Name = "Test", Url = "https://myservice:7777" };
        var entry2 = new RegistryEntryDto { Name = "Test2", Url = "https://myservice2:7777" };

        // Act
        await service.RegisterAsync(entry1);
        await service.RegisterAsync(entry2);
        IEnumerable<RegistryEntryDto> result = await service.FindRegistryEntriesAsync("Test");

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
