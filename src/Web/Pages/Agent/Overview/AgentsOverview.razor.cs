using AyBorg.Web.Services.Agent;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Agent.Overview;

public partial class AgentsOverview : ComponentBase, IAsyncDisposable
{
    [Inject] IAgentOverviewService AgentOverviewService { get; set; } = null!;

    private bool _isLoading = true;
    private Task _updateTask = null!;
    private bool _terminated = false;
    private bool _isDisposed = false;

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
                    _isLoading = false;
                    await InvokeAsync(StateHasChanged);
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            });
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }

    private async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            _terminated = true;
            if (_updateTask != null)
            {
                await _updateTask;
                _updateTask?.Dispose();
            }
            _isDisposed = true;
        }
    }
}
