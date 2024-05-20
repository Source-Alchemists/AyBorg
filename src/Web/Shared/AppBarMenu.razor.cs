using AyBorg.SDK.System;
using AyBorg.Web.Services;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Shared;

public partial class AppBarMenu : ComponentBase
{
    [Inject] ILogger<AppBarMenu> Logger { get; set; } = null!;
    [Inject] IRegistryService RegistryService { get; set; } = null!;
    [Inject] NavigationManager NavigationManager { get; set; } = null!;

    private const string DASHBOARD_URI = "/dashboard";
    private const string AGENTS_URI = "/agents/overview";
    private const string AGENTS_BASE_URI = "/agents/";
    private const string COGNITIVE_URI = "/cognitive/projects";
    private const string COGNITIVE_BASE_URI = "/cognitive/";
    private const string OBSERVABILITY_URI = "/observability/services";
    private const string OBSERVABILITY_BASE_URI = "/observability/";
    private const string AUDIT_URI = "/audit/reports";
    private const string AUDIT_BASE_URI = "/audit/";

    private bool _isDashboardAvailable = false;
    private bool _isCognitiveAvailable = false;
    private bool _isAuditAvailable = false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        NavigationManager.LocationChanged += (s, e) => Update();

        try
        {
            IEnumerable<Models.ServiceInfoEntry> services = await RegistryService.ReceiveServicesAsync();
            _isDashboardAvailable = services.Any(s => s.Type.Equals(ServiceTypes.Dashboard));
            _isCognitiveAvailable = services.Any(s => s.Type.Equals(ServiceTypes.Cognitive));
            _isAuditAvailable = services.Any(s => s.Type.Equals(ServiceTypes.Audit));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not retrieve analytics services from registry");
        }
    }

    private string GetActiveClass(string page)
    {
        if (NavigationManager.Uri.Contains(page))
        {
            return "app-bar-nav-item mud-chip-text mud-chip-color-secondary ml-1 mr-1";
        }
        else
        {
            return "app-bar-nav-item ml-1 mr-1";
        }
    }

    private void Update()
    {
        StateHasChanged();
    }
}
