using AyBorg.SDK.Data.DTOs;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Services.AppState;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Agent.Shared;

public sealed partial class ProjectSettingsDialog : ComponentBase
{
    [Parameter] public ProjectMetaDto ProjectMeta { get; set; } = null!;
    [Inject] IProjectManagementService ProjectManagementService { get; set; } = null!;
    [Inject] IStateService StateService { get; set; } = null!;

    private ProjectSettingsDto _projectSettings = null!;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        _projectSettings = await ProjectManagementService.GetProjectSettingsAsync(StateService.AgentState.BaseUrl, ProjectMeta);
    }
}
