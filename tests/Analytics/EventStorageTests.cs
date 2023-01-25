using AyBorg.Data.Analytics;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AyBorg.Analytics.Services.Tests;

public class EventStorageTests
{
    private readonly EventStorage _storage;
    private readonly IConfiguration _configuration;
    private readonly Mock<IEventLogRepository> _mockRepository = new();

    public EventStorageTests()
    {
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(
            initialData: new List<KeyValuePair<string, string>> {
                new("AyBorg:EventStorage:MaxDaysToKeep", "10")
            }!).Build();

        _storage = new EventStorage(_configuration, _mockRepository.Object);
    }

    [Fact]
    public void Test_Add()
    {
        // Arrange
        var testRecord = new EventRecord();
        _mockRepository.Setup(m => m.TryDelete(It.IsAny<IEnumerable<EventRecord>>())).Returns(true);
        _mockRepository.Setup(m => m.TryAdd(It.IsAny<EventRecord>())).Returns(true);
        // Act
        _storage.Add(testRecord);

        // Assert
        _mockRepository.Verify(m => m.TryDelete(It.IsAny<IEnumerable<EventRecord>>()));
        _mockRepository.Verify(m => m.TryAdd(testRecord));
    }

    [Fact]
    public void Test_GetRecords()
    {
        // Arrange
        var testRecord = new EventRecord();
        _mockRepository.Setup(m => m.FindAll()).Returns(new List<EventRecord> { testRecord });

        // Act
        IEnumerable<EventRecord> result = _storage.GetRecords();

        // Assert
        Assert.Single(result);
    }
}
