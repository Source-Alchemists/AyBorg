using AyBorg.SDK.System;
using AyBorg.Web.Services;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Shared;

public partial class NavMenu : ComponentBase
{
    [Inject] ILogger<NavMenu> Logger { get; set; } = null!;
    [Inject] IStateService StateService { get; set; } = null!;
    [Inject] IRegistryService RegistryService { get; set; } = null!;


    private bool _isAnalyticsVisible = false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        try
        {
            IEnumerable<Models.ServiceInfoEntry> analyticsServices = await RegistryService.ReceiveServicesAsync(ServiceTypes.Analytics);
            _isAnalyticsVisible = analyticsServices.Any();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not retrieve analytics services from registry");
        }
    }

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
