using Atomy.Web.Shared.Models;
using Blazored.LocalStorage;

namespace Atomy.Web.Services;

public class StateService : IStateService
{
    private readonly ILocalStorageService _localStorageService;

    public Action OnUpdate { get; set; } = null!;

    public UiAgentState AgentState { get; set; } = null!;

    public StateService(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public async Task UpdateFromLocalstorageAsync()
    {
        var result = await _localStorageService.GetItemAsync<UiAgentState>("AgentState");
        if (result != null)
        {
            var lastUrl = AgentState == null ? string.Empty : AgentState.BaseUrl;
            AgentState = result;
            if (!string.IsNullOrEmpty(lastUrl))
            {
                AgentState.BaseUrl = lastUrl;
            }
            await RefreshAsync();
        }
    }

    public async Task RefreshAsync()
    {
        await _localStorageService.SetItemAsync("AgentState", AgentState);
        OnUpdate?.Invoke();
    }
}