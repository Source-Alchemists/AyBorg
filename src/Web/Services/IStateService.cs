using Autodroid.Web.Shared.Models;

namespace Autodroid.Web.Services;

public interface IStateService
{
    Action OnUpdate { get; set; }
    UiAgentState AgentState { get; set; }
    Task UpdateFromLocalstorageAsync();
    Task RefreshAsync();
}