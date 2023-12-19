using AyBorg.SDK.System;
using AyBorg.Web.Services;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Shared;

public partial class NavMenu : ComponentBase
{
    [Inject] ILogger<NavMenu> Logger { get; set; } = null!;
    [Inject] IStateService StateService { get; set; } = null!;
    [Inject] IRegistryService RegistryService { get; set; } = null!;
    [Inject] NavigationManager NavigationManager { get; set; } = null!;

    private bool _isDashboardAvailable = false;
    private bool _isAnyAgentAvailable = false;
    private bool _isNetAvailable = false;
    private bool _isAnalyticsAvailable = false;
    private bool _isAuditAvailable = false;

    private const string PRO_VERSION = " - PRO";

    private string _dashboardTitle = "Dashboard";
    private string _netTitle = "Artificial Intelligence";
    private string _auditTitle = "Audit";

    private Category _category = Category.None;


    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        UpdateCategory();
        NavigationManager.LocationChanged += (s, e) => OnUpdate();

        try
        {
            IEnumerable<Models.ServiceInfoEntry> services = await RegistryService.ReceiveServicesAsync();

            _isDashboardAvailable = services.Any(s => s.Type.Equals(ServiceTypes.Dashboard));
            _isAnyAgentAvailable = services.Any(s => s.Type.Equals(ServiceTypes.Agent));
            _isNetAvailable = services.Any(s => s.Type.Equals(ServiceTypes.Net));
            _isAnalyticsAvailable = services.Any(s => s.Type.Equals(ServiceTypes.Log));
            _isAuditAvailable = services.Any(s => s.Type.Equals(ServiceTypes.Audit));

            if(!_isDashboardAvailable)
            {
                _dashboardTitle = $"{_dashboardTitle}{PRO_VERSION}";
            }

            if(!_isNetAvailable)
            {
                _netTitle = $"{_netTitle}{PRO_VERSION}";
            }

            if(!_isAuditAvailable)
            {
                _auditTitle = $"{_auditTitle}{PRO_VERSION}";
            }
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
        UpdateCategory();
        await InvokeAsync(StateHasChanged);
    }

    private void UpdateCategory()
    {
        string uri = NavigationManager.Uri;
        if(uri.Contains("/dashboard"))
        {
            _category = Category.Dashboard;
            return;
        }

        if(uri.Contains("/agents/"))
        {
            _category = Category.Agents;
            return;
        }

        if(uri.Contains("/net/"))
        {
            _category = Category.Net;
            return;
        }

        if(uri.Contains("/observability/"))
        {
            _category = Category.Observability;
            return;
        }

        if(uri.Contains("/audit/"))
        {
            _category = Category.Audit;
            return;
        }

        if(uri.Contains("/admin/"))
        {
            _category = Category.Admin;
            return;
        }

        _category = Category.None;
    }

    private enum Category 
    {
        None,
        Dashboard,
        Agents,
        Net,
        Observability,
        Audit,
        Admin
    }
}
