using Autodroid.Web.Shared.Models;

namespace Autodroid.Web.Services;

public interface IStateService
{
    Action OnUpdate { get; set; }
    UiAgentState AgentState { get; }
    double AutomationFlowZoom { get; }
    Task UpdateAgentStateFromLocalstorageAsync();
    Task SetAgentStateAsync(UiAgentState agentState);
    Task SetAutomationFlowZoomAsync(double zoom);
    Task<double> UpdateAutomationFlowZoomFromLocalstorageAsync();
}