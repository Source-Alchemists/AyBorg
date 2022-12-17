using AyBorg.Web.Shared.Models;
using Blazored.LocalStorage;
using Blazored.SessionStorage;

namespace AyBorg.Web.Services.AppState;

public class StateService : IStateService
{
    private readonly ISessionStorageService _sessionStorageService;
    private readonly ILocalStorageService _localStorageService;

    public Action OnUpdate { get; set; } = null!;

    public UiAgentState AgentState { get; private set; } = null!;

    public AutomationFlowState AutomationFlowState { get; private set; } = null!;

    public StateService(ISessionStorageService sessionStorageService, ILocalStorageService localStorageService)
    {
        _sessionStorageService = sessionStorageService;
        _localStorageService = localStorageService;
        AutomationFlowState = new AutomationFlowState(_localStorageService);
    }

    public async Task UpdateAgentStateFromLocalstorageAsync()
    {
        UiAgentState result = await _sessionStorageService.GetItemAsync<UiAgentState>("Agent_State");
        if (result != null)
        {
            await SetAgentStateAsync(result);
        }
    }

    public async Task SetAgentStateAsync(UiAgentState agentState)
    {
        AgentState = agentState;
        await _sessionStorageService.SetItemAsync("Agent_State", agentState);
        OnUpdate?.Invoke();
    }
}
