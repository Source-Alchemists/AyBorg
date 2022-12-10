using AyBorg.Web.Services.Agent;
using AyBorg.Web.Services.AppState;
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

        _projectSettings = await ProjectSettingsService.GetProjectSettingsAsync(StateService.AgentState.BaseUrl, ProjectMeta);
    }

    private async Task ChangeResultCommunicationClicked()
    {
        IDialogReference dialog = DialogService.Show<ConfirmDialog>("Change result communication",
                                            new DialogParameters {
                                                { "NeedPassword", true },
                                                { "ContentText", "Are you sure you want to change the result communication?" }
                                            });
        DialogResult result = await dialog.Result;
        if (!result.Cancelled)
        {
            _projectSettings.IsForceResultCommunicationEnabled = !_projectSettings.IsForceResultCommunicationEnabled;
            await UpdateCommunicationSettings();
        }
    }

    private async Task ChangeUiCommunicationClicked()
    {
        IDialogReference dialog = DialogService.Show<ConfirmDialog>("Change UI communication",
                                            new DialogParameters {
                                                { "NeedPassword", true },
                                                { "ContentText", "Are you sure you want to change the UI communication?" }
                                            });
        DialogResult result = await dialog.Result;
        if (!result.Cancelled)
        {
            _projectSettings.IsForceWebUiCommunicationEnabled = !_projectSettings.IsForceWebUiCommunicationEnabled;
            await UpdateCommunicationSettings();
        }
    }

    private async ValueTask UpdateCommunicationSettings()
    {
        bool apiResult = await ProjectSettingsService.TryUpdateProjectCommunicationSettingsAsync(StateService.AgentState.BaseUrl, ProjectMeta, _projectSettings);
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
