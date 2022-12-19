namespace AyBorg.Web.Services;

public interface INotifyService
{
    Action<Guid> AgentIterationFinished { get; set; }
}
