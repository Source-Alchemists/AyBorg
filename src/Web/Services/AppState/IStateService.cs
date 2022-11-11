using Autodroid.Web.Shared.Models;

namespace Autodroid.Web.Services.AppState;

public interface IStateService
{
    Action OnUpdate { get; set; }
    UiAgentState AgentState { get; }
    AutomationFlowState AutomationFlowState { get; }
    Task SetAgentStateAsync(UiAgentState agentState);
    Task UpdateAgentStateFromLocalstorageAsync();
}