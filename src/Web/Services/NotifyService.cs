namespace AyBorg.Web.Services;

public sealed class NotifyService : INotifyService
{
    public Action<Guid> AgentIterationFinished { get; set; } = null!;
}
