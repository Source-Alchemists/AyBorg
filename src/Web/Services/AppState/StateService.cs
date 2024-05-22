using AyBorg.Web.Shared.Models;
using Blazored.LocalStorage;
using Blazored.SessionStorage;

namespace AyBorg.Web.Services;

public class StateService : IStateService
{
    private readonly ISessionStorageService _sessionStorageService;

    public Action OnUpdate { get; set; } = null!;

    public UiAgentState AgentState { get; private set; } = null!;
    public UiCognitiveState CognitiveState { get; private set; } = null!;

    public AutomationFlowState AutomationFlowState { get; private set; }

    public StateService(ISessionStorageService sessionStorageService, ILocalStorageService localStorageService)
    {
        _sessionStorageService = sessionStorageService;
        AutomationFlowState = new AutomationFlowState(localStorageService);
    }

    public async ValueTask UpdateStateFromSessionStorageAsync()
    {
        UiAgentState agentState = await _sessionStorageService.GetItemAsync<UiAgentState>("Agent_State");
        UiCognitiveState netState = await _sessionStorageService.GetItemAsync<UiCognitiveState>("Cognitive_State");
        if (agentState != null)
        {
            await SetAgentStateAsync(agentState);
        }

        if(netState != null)
        {
            await SetNetStateAsync(netState);
        }
    }

    public async ValueTask SetAgentStateAsync(UiAgentState agentState)
    {
        AgentState = agentState;
        await _sessionStorageService.SetItemAsync("Agent_State", agentState);
        OnUpdate?.Invoke();
    }

    public async ValueTask SetNetStateAsync(UiCognitiveState netState)
    {
        CognitiveState = netState;
        await _sessionStorageService.SetItemAsync("Cognitive_State", netState);
        OnUpdate?.Invoke();
    }
}
