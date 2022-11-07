using Autodroid.Web.Shared.Models;
using Blazored.LocalStorage;

namespace Autodroid.Web.Services;

public class StateService : IStateService
{
    private readonly ILocalStorageService _localStorageService;

    public Action OnUpdate { get; set; } = null!;

    public UiAgentState AgentState { get; private set; } = null!;

    public double AutomationFlowZoom { get; private set; } = 1.0;

    public StateService(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public async Task UpdateAgentStateFromLocalstorageAsync()
    {
        var result = await _localStorageService.GetItemAsync<UiAgentState>("AgentState");
        if (result != null)
        {
            var lastUrl = AgentState == null ? string.Empty : AgentState.BaseUrl;
            if (!string.IsNullOrEmpty(lastUrl))
            {
                result.BaseUrl = lastUrl;
            }
            await SetAgentStateAsync(result);
        }
    }

    public async Task SetAgentStateAsync(UiAgentState agentState)
    {
        AgentState = agentState;
        await _localStorageService.SetItemAsync("AgentState", agentState);
        OnUpdate?.Invoke();
    }

    public async Task SetAutomationFlowZoomAsync(double zoom)
    {
        AutomationFlowZoom = zoom;
        await _localStorageService.SetItemAsync("AutomationFlowZoom", zoom);
    }

    public async Task<double> UpdateAutomationFlowZoomFromLocalstorageAsync()
    {
        var result = await _localStorageService.GetItemAsync<double>("AutomationFlowZoom");
        if (result != 0)
        {
            AutomationFlowZoom = result;
            return result;
        }
        result = 1.0;
        return result;
    }
}