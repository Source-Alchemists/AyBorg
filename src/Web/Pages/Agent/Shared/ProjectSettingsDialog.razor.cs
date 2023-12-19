using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Shared.Modals;
using AyBorg.Web.Shared.Models.Agent;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Shared;

public sealed partial class ProjectSettingsDialog : ComponentBase
{
    [Parameter] public ProjectMeta ProjectMeta { get; set; } = null!;
    [Inject] IProjectSettingsService ProjectSettingsService { get; set; } = null!;
    [Inject] IStateService StateService { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;
    [Inject] ISnackbar Snackbar { get; set; } = null!;

    private ProjectSettings _projectSettings = null!;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        _projectSettings = await ProjectSettingsService.GetProjectSettingsAsync(StateService.AgentState.UniqueName, ProjectMeta);
    }

    private async Task ChangeResultCommunicationClicked()
    {
        IDialogReference dialog = DialogService.Show<ConfirmDialog>("Change result communication",
                                            new DialogParameters {
                                                { "NeedPassword", true },
                                                { "ContentText", "Are you sure you want to change the result communication?" }
                                            });
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            _projectSettings.IsForceResultCommunicationEnabled = !_projectSettings.IsForceResultCommunicationEnabled;
            await UpdateCommunicationSettings();
        }
    }

    private async ValueTask UpdateCommunicationSettings()
    {
        bool apiResult = await ProjectSettingsService.TryUpdateProjectCommunicationSettingsAsync(StateService.AgentState.UniqueName, ProjectMeta, _projectSettings);
        if (apiResult)
        {
            Snackbar.Add("Result communication changed successfully.", Severity.Success);
        }
        else
        {
            Snackbar.Add("Result communication change failed.", Severity.Error);
        }

        await InvokeAsync(StateHasChanged);
    }
}
