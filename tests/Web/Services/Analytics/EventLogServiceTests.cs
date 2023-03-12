using Ayborg.Gateway.Analytics.V1;
using AyBorg.Web.Shared.Models;
using AyBorg.Web.Tests.Helpers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Moq;

namespace AyBorg.Web.Services.Analytics.Tests;

public class EventLogServiceTests
{
    private readonly Mock<EventLog.EventLogClient> _mockEventLogClient = new();
    private readonly EventLogService _service;

    public EventLogServiceTests()
    {
        _service = new EventLogService(_mockEventLogClient.Object);
    }

    [Fact]
    public async Task Test_GetEventsAsync()
    {
        // Arrange
        var testEntry = new EventEntry
        {
            ServiceType = "Test_ServiceType",
            ServiceUniqueName = "Test_ServiceUniqueName",
            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
            LogLevel = 1,
            Message = "Test_Message",
            EventId = 2
        };

        AsyncServerStreamingCall<EventEntry> callStream = GrpcCallHelpers.CreateAsyncServerStreamingCall(new List<EventEntry> {
            testEntry
        });

        _mockEventLogClient.Setup(m => m.GetLogEvents(It.IsAny<GetEventsRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(callStream);

        // Act
        await foreach (EventLogEntry result in _service.GetEventsAsync())
        {
            // Assert
            Assert.Equal(testEntry.ServiceType, result.ServiceType);
            Assert.Equal(testEntry.ServiceUniqueName, result.ServiceUniqueName);
            Assert.Equal(testEntry.LogLevel, (int)result.LogLevel);
            Assert.Equal(testEntry.Message, result.Message);
            Assert.Equal(testEntry.EventId, result.EventId);
            Assert.Equal(testEntry.Timestamp, Timestamp.FromDateTime(result.Timestamp));
        }
    }
}
