using Ayborg.Gateway.Analytics.V1;
using AyBorg.Analytics.Helpers.Tests;
using AyBorg.Data.Analytics;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Analytics.Services.Tests;

public class EventServiceV1Tests
{

    private static readonly NullLogger<EventLogServiceV1> s_logger = new();
    private readonly Mock<IEventStorage> _mockEventStorage = new();
    protected readonly TestServerCallContext _serverCallContext;
    private readonly EventLogServiceV1 _service;

    public EventServiceV1Tests()
    {
        _serverCallContext = TestServerCallContext.Create(null, new CancellationTokenSource().Token);
        _mockEventStorage.Setup(m => m.GetRecords()).Returns(new List<EventRecord> {
            new EventRecord {
                Timestamp = DateTime.UtcNow
            }
        });
        _service = new EventLogServiceV1(s_logger, _mockEventStorage.Object);
    }

    [Fact]
    public async Task Test_LogEvent()
    {
        // Arrange
        var testEntry = new EventEntry
        {
            ServiceType = "Test_ServiceType",
            ServiceUniqueName = "Test_ServiceUniqueName",
            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
            LogLevel = 1,
            EventId = 2,
            Message = "Test_Message"
        };

        // Act
        Empty result = await _service.LogEvent(testEntry, _serverCallContext);

        // Assert
        Assert.NotNull(result);
        _mockEventStorage.Verify(m => m.Add(It.IsAny<EventRecord>()));
    }

    [Fact]
    public async Task Test_GetLogEvents()
    {
         // Arrange
        var request = new GetEventsRequest {
            ServiceType = "Test_ServiceType",
            ServiceUniqueName = "Test_ServiceUniqueName",
            LogLevel = 1,
            EventId = 2
        };

        var mockServerStreamWriter = new Mock<IServerStreamWriter<EventEntry>>();

        // Act
        await _service.GetLogEvents(request, mockServerStreamWriter.Object, _serverCallContext);

        // Assert
        mockServerStreamWriter.Verify(w => w.WriteAsync(It.IsAny<EventEntry>()));
    }
}
