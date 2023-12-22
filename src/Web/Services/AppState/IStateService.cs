using AyBorg.Web.Shared.Models;

namespace AyBorg.Web.Services;

public interface IStateService
{
    Action OnUpdate { get; set; }
    UiAgentState AgentState { get; }
    UiNetState NetState { get; }
    AutomationFlowState AutomationFlowState { get; }
    ValueTask SetAgentStateAsync(UiAgentState agentState);
    ValueTask SetNetStateAsync(UiNetState netState);
    ValueTask UpdateStateFromSessionStorageAsync();
}
