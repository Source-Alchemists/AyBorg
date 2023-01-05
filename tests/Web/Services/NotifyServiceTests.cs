using AyBorg.Web.Services;

namespace AyBorg.Web.Tests.Services;

public class NotifyServiceTests
{
    private readonly NotifyService _service = new();

    [Fact]
    public void Test_SubscribeAndUnsubscribe()
    {
        NotifyService.Subscription subscription = _service.Subscribe("Test", SDK.Communication.gRPC.NotifyType.AgentAutomationFlowChanged);
        Assert.Single(_service.Subscriptions);
        _service.Unsubscribe(subscription);
        Assert.Empty(_service.Subscriptions);
    }
}
