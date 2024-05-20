using AyBorg.Data.Gateway;
using AyBorg.Gateway.Models;
using AyBorg.SDK.System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Gateway.Services.Tests;

public sealed class KeeperServiceTests : IDisposable
{
    private static readonly NullLogger<KeeperService> s_logger = new();
    private readonly NullLogger<GatewayConfiguration> _registryConfigurationLogger = new();
    private readonly IConfiguration _configuration;
    private readonly IGatewayConfiguration _registryConfiguration;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly DbContextOptions<RegistryContext> _contextOptions;
    private readonly Mock<IGrpcChannelService> _mockGrpcChannelService = new();

    private readonly KeeperService _service;
    private readonly ServiceEntry _validServiceEntry = new() { Name = "Test", UniqueName = "Test-1", Url = "https://myservice:7777" };

    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeeperServiceTests"/> class.
    /// </summary>
    public KeeperServiceTests()
    {
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(
            initialData: new List<KeyValuePair<string, string>> {
                new("Kestrel:Endpoints:gRPC:Url", "https://localhost:5001"),
                new("AyBorg:Service:Url", "https://localhost:5001")
            }!).Build();

        _registryConfiguration = new GatewayConfiguration(_registryConfigurationLogger, _configuration);

        _connection = new Microsoft.Data.Sqlite.SqliteConnection("Filename=:memory:");
        _connection.Open();
        _contextOptions = new DbContextOptionsBuilder<RegistryContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new RegistryContext(_contextOptions);
        context.Database.EnsureCreated();

        _mockGrpcChannelService = new Mock<IGrpcChannelService>();
        _mockGrpcChannelService.Setup(c => c.TryRegisterChannel(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        _service = new KeeperService(s_logger, _configuration, _registryConfiguration, CreateContextFactoryMock().Object, _mockGrpcChannelService.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("http://test.org")]
    [InlineData("https://test.org")]
    public void Test_ConfigurationServerUrl(string url)
    {
        // Arrange
        IConfigurationRoot configuration = new ConfigurationBuilder().AddInMemoryCollection(
            initialData: new List<KeyValuePair<string, string>> {
                new("Kestrel:Endpoints:gRPC:Url", url),
                new("AyBorg:Service:Url", url)
            }!).Build();
        var registryConfiguration = new GatewayConfiguration(_registryConfigurationLogger, configuration);

        // Act
        KeeperService service;
        if (string.IsNullOrEmpty(url))
        {
            Assert.Throws<InvalidOperationException>(() => service = new KeeperService(s_logger, configuration, registryConfiguration, CreateContextFactoryMock().Object, _mockGrpcChannelService.Object));
        }
        else
        {
            service = new KeeperService(s_logger, configuration, registryConfiguration, CreateContextFactoryMock().Object, _mockGrpcChannelService.Object);

            // Assert
            Assert.NotNull(service);
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Test_RegisterAsync(bool registerTwice, bool unregister)
    {
        // Arrange
        // Act
        Guid result = await _service.RegisterAsync(_validServiceEntry);
        if (unregister)
        {
            _service.Unregister(result);
        }
        if (registerTwice)
        {
            _mockGrpcChannelService.Setup(c => c.TryRegisterChannel(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterAsync(_validServiceEntry));
        }

        // Assert
        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact]
    public void Test_UnregisterAsync_unknownService()
    {
        // Arrange
        // Assert
        Assert.Throws<KeyNotFoundException>(() => _service.Unregister(Guid.Empty));
    }

    [Fact]
    public async Task Test_GetAllServicRegistryEntriesAsync()
    {
        // Arrange
        var entry2 = new ServiceEntry { Name = "Test2", UniqueName = "Test2", Url = "https://myservice2:7777" };

        // Act
        await _service.RegisterAsync(_validServiceEntry);
        await _service.RegisterAsync(entry2);
        IEnumerable<ServiceEntry> result = await _service.GetAllRegistryEntriesAsync();

        // Assert
        Assert.Equal(3, result.Count());
        Assert.Contains(_validServiceEntry, result);
        Assert.Contains(entry2, result);
    }

    [Fact]
    public async Task Test_FindRegistryEntriesAsync()
    {
        // Arrange
        var entry2 = new ServiceEntry { Name = "Test2", UniqueName = "Test2", Url = "https://myservice2:7777" };

        // Act
        await _service.RegisterAsync(_validServiceEntry);
        await _service.RegisterAsync(entry2);
        IEnumerable<ServiceEntry> result = await _service.FindRegistryEntriesAsync("Test");

        // Assert
        Assert.Single(result);
        Assert.Contains(_validServiceEntry, result);
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
            _service.Dispose();
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
