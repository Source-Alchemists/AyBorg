using AyBorg.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace AyBorg.Web.Tests.Services;

public class NotifyServiceTests
{
    private static readonly NullLogger<NotifyService> s_logger = new();
    private readonly NotifyService _service;

    public NotifyServiceTests()
    {
        _service = new NotifyService(s_logger);
    }

    [Fact]
    public void Test_SubscribeAndUnsubscribe()
    {
        NotifyService.Subscription subscription = _service.Subscribe("Test", SDK.Communication.gRPC.NotifyType.AgentAutomationFlowChanged);
        Assert.Single(_service.Subscriptions);
        _service.Unsubscribe(subscription);
        Assert.Empty(_service.Subscriptions);
    }
}
