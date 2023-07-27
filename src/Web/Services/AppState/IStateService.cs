using AyBorg.Web.Shared.Models;

namespace AyBorg.Web.Services;

public interface IStateService
{
    Action OnUpdate { get; set; }
    UiAgentState AgentState { get; }
    AutomationFlowState AutomationFlowState { get; }
    Task SetAgentStateAsync(UiAgentState agentState);
    Task UpdateAgentStateFromSessionStorageAsync();
}
