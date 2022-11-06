using Microsoft.AspNetCore.Components;
using Autodroid.Web.Services.Agent;

namespace Autodroid.Web.Pages.Agent.Overview;

public partial class AgentsOverview : ComponentBase, IAsyncDisposable
{
    [Inject] IAgentOverviewService AgentOverviewService { get; set; } = null!;

    private Task _updateTask = null!;
    private bool _terminated = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            _updateTask = Task.Run(async () =>
            {
                while (!_terminated)
                {
                    await AgentOverviewService.UpdateAsync();
                    await InvokeAsync(StateHasChanged);
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            });
        }
    }

    public async ValueTask DisposeAsync()
    {
        _terminated = true;
        if(_updateTask != null)
        {
            await _updateTask;
        }
    }
}