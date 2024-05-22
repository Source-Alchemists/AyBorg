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
    private bool _isNetAvailable = false;
    private bool _isAnalyticsAvailable = false;
    private bool _isAuditAvailable = false;

    private const string PRO_VERSION = " - PRO";

    private string _dashboardTitle = "Dashboard";
    private string _netTitle = "Artificial Intelligence";
    private string _auditTitle = "Audit";

    private Category _category = Category.None;
    private bool _isAgentSelected => StateService.AgentState != null;
    private string _agentName => _isAgentSelected ? StateService.AgentState.Name : "-";
    private string _agentProjectLink => _isAgentSelected ? StateService.AgentState.ProjectsLink : string.Empty;
    private string _agentEditorLink => _isAgentSelected ? StateService.AgentState.EditorLink : string.Empty;
    private string _agentDevicesLink => _isAgentSelected ? StateService.AgentState.DevicesLink : string.Empty;

    private bool _isCognitiveProjectSelected => StateService.CognitiveState != null && !string.IsNullOrEmpty(StateService.CognitiveState.ProjectId);
    private string _cognitiveProjectName => _isCognitiveProjectSelected ?  StateService.CognitiveState.ProjectName : "-";
    private string _cognitiveUploadLink => _isCognitiveProjectSelected ? $"cognitive/upload/{StateService.CognitiveState.ProjectId}" : string.Empty;
    private string _cognitiveAnnotateLink => _isCognitiveProjectSelected ? $"cognitive/browse/{StateService.CognitiveState.ProjectId}" : string.Empty;
    private string _cognitiveDatasetsLink => _isCognitiveProjectSelected ? $"cognitive/datasets/{StateService.CognitiveState.ProjectId}" : string.Empty;
    private string _cognitiveModelsLink => _isCognitiveProjectSelected ? $"cognitive/models/{StateService.CognitiveState.ProjectId}" : string.Empty;


    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        UpdateCategory();
        NavigationManager.LocationChanged += (s, e) => OnUpdate();

        try
        {
            IEnumerable<Models.ServiceInfoEntry> services = await RegistryService.ReceiveServicesAsync();

            _isDashboardAvailable = services.Any(s => s.Type.Equals(ServiceTypes.Dashboard));
            _isNetAvailable = services.Any(s => s.Type.Equals(ServiceTypes.Cognitive));
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
            await StateService.UpdateStateFromSessionStorageAsync();
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

        if(uri.Contains("/cognitive/"))
        {
            _category = Category.Cognitive;
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
        Cognitive,
        Observability,
        Audit,
        Admin
    }
}
