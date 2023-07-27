using AyBorg.SDK.System;
using AyBorg.Web.Services;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Shared;

public partial class NavMenu : ComponentBase
{
    [Inject] ILogger<NavMenu> Logger { get; set; } = null!;
    [Inject] IStateService StateService { get; set; } = null!;
    [Inject] IRegistryService RegistryService { get; set; } = null!;

    private bool _isAnyAgentAvailable = false;
    private bool _isAnalyticsAvailable = false;
    private bool _isAuditAvailable = false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        try
        {
            IEnumerable<Models.ServiceInfoEntry> services = await RegistryService.ReceiveServicesAsync();

            _isAnyAgentAvailable = services.Any(s => s.Type.Equals(ServiceTypes.Agent));
            _isAnalyticsAvailable = services.Any(s => s.Type.Equals(ServiceTypes.Analytics));
            _isAuditAvailable = services.Any(s => s.Type.Equals(ServiceTypes.Audit));
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
            await StateService.UpdateAgentStateFromSessionStorageAsync();
        }
    }

    private async void OnUpdate()
    {
        await InvokeAsync(StateHasChanged);
    }
}
