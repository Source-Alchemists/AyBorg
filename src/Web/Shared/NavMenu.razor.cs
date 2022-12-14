using AyBorg.Web.Services.AppState;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Shared;

public partial class NavMenu : ComponentBase
{
    [Inject] IStateService StateService { get; set; } = null!;
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            StateService.OnUpdate += OnUpdate;
            await StateService.UpdateAgentStateFromLocalstorageAsync();
        }
    }

    private async void OnUpdate()
    {
        await InvokeAsync(StateHasChanged);
    }
}