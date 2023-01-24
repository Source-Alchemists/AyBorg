using Ayborg.Gateway.Analytics.V1;
using AyBorg.Gateway.Models;
using AyBorg.Gateway.Services.Tests;
using AyBorg.Gateway.Tests.Helpers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Moq;

namespace AyBorg.Gateway.Services.Analytics.Tests;

public class EventLogPassthroughServiceV1Tests : BaseGrpcServiceTests<EventLogPassthroughServiceV1, EventLog.EventLogClient>
{
    public EventLogPassthroughServiceV1Tests()
    {
        _mockGrpcChannelService.Setup(m => m.GetChannelsByTypeName(It.IsAny<string>())).Returns(new List<ChannelInfo> {
            new ChannelInfo()
        });
        _service = new EventLogPassthroughServiceV1(_mockGrpcChannelService.Object);
    }

    [Fact]
    public async ValueTask Test_LogEvent()
    {
        // Act
        Empty result = await _service.LogEvent(new EventEntry{
            ServiceType = "Test_ServiceType",
            ServiceUniqueName = "Test_ServiceUniqueName",
            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
            LogLevel = 1,
            Message = "Test_Message",
            EventId = 2
        }, _serverCallContext);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async ValueTask Test_GetLogEvents()
    {
        // Arrange
        var request = new GetEventsRequest {
            ServiceType = "Test_ServiceType",
            ServiceUniqueName = "Test_ServiceUniqueName",
            LogLevel = 1,
            EventId = 2
        };
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
        _mockClient.Setup(m => m.GetLogEvents(It.IsAny<GetEventsRequest>(), null, null, It.IsAny<CancellationToken>())).Returns(callStream);
        var mockServerStreamWriter = new Mock<IServerStreamWriter<EventEntry>>();

        // Act
        await _service.GetLogEvents(request, mockServerStreamWriter.Object, _serverCallContext);

        // Assert
        mockServerStreamWriter.Verify(w => w.WriteAsync(It.IsAny<EventEntry>(), It.IsAny<CancellationToken>()));
    }
}
