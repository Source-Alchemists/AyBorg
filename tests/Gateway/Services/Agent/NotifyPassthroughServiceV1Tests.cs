using Ayborg.Gateway.Agent.V1;
using AyBorg.Gateway.Models;
using AyBorg.Gateway.Services.Tests;
using Grpc.Core;
using Moq;

namespace AyBorg.Gateway.Services.Agent.Tests;

public class NotifyPassthroughServiceV1Tests : BaseGrpcServiceTests<NotifyPassthroughServiceV1, Notify.NotifyClient>
{
    public NotifyPassthroughServiceV1Tests()
    {
        _service = new NotifyPassthroughServiceV1(s_logger, _mockGrpcChannelService.Object);
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public async Task Test_CreateDownstream(bool hasValidChannelInfo, int notifyCount)
    {
        // Arrange
        ChannelInfo channelInfo = null!;
        if (hasValidChannelInfo)
        {
            channelInfo = new ChannelInfo { ServiceUniqueName = "Test", TypeName = "Test" };
        }

        for (int index = 0; index < notifyCount; index++)
        {
            channelInfo.Notifications.Add(new Notification("Test", SDK.Communication.gRPC.NotifyType.AgentAutomationFlowChanged, string.Empty));
        }

        _mockGrpcChannelService.Setup(c => c.GetChannelByName(It.IsAny<string>())).Returns(channelInfo);
        var request = new CreateNotifyStreamRequest();

        var mockServerStreamWriter = new Mock<IServerStreamWriter<NotifyMessage>>();

        // Act
        Task streamTask = _service.CreateDownstream(request, mockServerStreamWriter.Object, _serverCallContext);

        if (notifyCount > 0)
        {
            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromSeconds(5));
            bool notified = false;
            _service.DownstreamNotified += (m) =>
            {
                notified = true;
            };
            while (!notified && !tokenSource.Token.IsCancellationRequested)
            {
                await Task.Delay(1);
            }

        }
        _serverCallContextCancellationTokenSource.Cancel();
        await streamTask;

        // Assert
        mockServerStreamWriter.Verify(w => w.WriteAsync(It.IsAny<NotifyMessage>()), Times.Exactly(notifyCount));
    }

    [Theory]
    [InlineData(1, 0, true, 0)]
    [InlineData(1, 1, true, 1)]
    [InlineData(1, 1, false, 0)]
    [InlineData(1, 11, true, 10)]
    [InlineData(1, 11, false, 0)]
    [InlineData(2, 0, true, 0)]
    [InlineData(2, 1, true, 1)]
    [InlineData(2, 1, false, 0)]
    [InlineData(2, 11, true, 10)]
    [InlineData(2, 11, false, 0)]
    [InlineData(0, 0, true, 0)]
    public async Task Test_CreateNotificationFromAgent(int validChannelInfoCount, int notifyCount, bool isAcceptingNotifications, int expectedNotifyCount)
    {
        // Arrange
        var channelInfos = new List<ChannelInfo>();
        for (int index = 0; index < validChannelInfoCount; index++)
        {
            channelInfos.Add(new ChannelInfo
            {
                ServiceUniqueName = $"Test-{index}",
                TypeName = "AyBorg.Web",
                IsAcceptingNotifications = isAcceptingNotifications
            });
        }

        _mockGrpcChannelService.Setup(c => c.GetChannelsByTypeName(It.IsAny<string>())).Returns(channelInfos);

        // Act
        for (int index = 0; index < notifyCount; index++)
        {
            var request = new NotifyMessage { AgentUniqueName = "AyBorg.Agent", Type = 0, Payload = string.Empty };
            await _service.CreateNotificationFromAgent(request, _serverCallContext);
        }

        // Assert
        Assert.All(channelInfos, i => Assert.Equal(expectedNotifyCount, i.Notifications.Count));
    }
}
