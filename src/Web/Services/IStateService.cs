using Atomy.Web.Shared.Models;

namespace Atomy.Web.Services;

public interface IStateService
{
    Action OnUpdate { get; set; }
    UiAgentState AgentState { get; set; }
    Task UpdateFromLocalstorageAsync();
    Task RefreshAsync();
}