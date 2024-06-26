using System.Collections.Immutable;
using AyBorg.SDK.Common;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Cognitive;
using AyBorg.Web.Shared.Models.Cognitive;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace AyBorg.Web.Pages.Cognitive.Projects;

#nullable disable

public partial class NewProjectDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [CascadingParameter] Task<AuthenticationState> AuthenticationState { get; set; }
    [Inject] ILogger<NewProjectDialog> Logger { get; init; }
    [Inject] IProjectManagerService ProjectManagerService { get; init; }
    [Inject] IStateService StateService { get; init; }
    [Inject] NavigationManager NavigationManager { get; init; }
    [Inject] ISnackbar Snackbar { get; init; }
    private string[] _projectTypes = Array.Empty<string>();
    private string _selectedProjectType = string.Empty;
    private ImmutableList<string> _addedTags = ImmutableList<string>.Empty;
    private string _tmpTag = string.Empty;
    private string _projectName = string.Empty;
    private string _username = string.Empty;
    private MudTextField<string> _tagField;
    private string _projectNameError;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (AuthenticationState is not null)
        {
            AuthenticationState authState = await AuthenticationState;
            System.Security.Claims.ClaimsPrincipal user = authState?.User;

            if (user?.Identity is not null)
            {
                _username = user.Identity.Name!;
            }
        }

        _projectTypes = new[] { ProjectType.ObjectDetection.GetDescription() };
        _selectedProjectType = _projectTypes[0];
    }

    private void TagAdornmentClicked()
    {
        if (string.IsNullOrEmpty(_tmpTag) || string.IsNullOrWhiteSpace(_tmpTag))
        {
            return;
        }

        string[] tags = _tmpTag.Split(',');

        foreach (string tag in tags)
        {
            string upperTag = tag.ToUpperInvariant().Trim();
            if (_addedTags.Exists(x => x.Equals(upperTag, StringComparison.InvariantCultureIgnoreCase)))
            {
                continue;
            }

            _addedTags = _addedTags.Add(upperTag);
        }

        _tmpTag = string.Empty;
    }

    private void TagRemoved(MudChip chip)
    {
        _addedTags = _addedTags.Remove(chip.Text);
    }

    private async Task TagsKeyUp(KeyboardEventArgs args)
    {
        if (args.Code.Equals("Space", StringComparison.InvariantCultureIgnoreCase)
            || args.Code.Equals("Enter", StringComparison.InvariantCultureIgnoreCase)
            || args.Code.Equals("NumpadEnter", StringComparison.InvariantCultureIgnoreCase)
            || args.Code.Equals("Tab", StringComparison.InvariantCultureIgnoreCase))
        {
            TagAdornmentClicked();
            await _tagField.BlurAsync();
            await _tagField.Clear();
            await _tagField.FocusAsync();
        }
    }

    private async Task CreateClicked()
    {
        _projectNameError = string.Empty;
        if (string.IsNullOrEmpty(_projectName) || string.IsNullOrWhiteSpace(_projectName))
        {
            _projectNameError = "Please enter a project name";
            return;
        }

        int selectedProjectTypeIndex = Array.FindIndex(_projectTypes, p => p.Equals(_selectedProjectType, StringComparison.InvariantCultureIgnoreCase));
        ImmutableList<string> tags = ImmutableList<string>.Empty;
        foreach (string addedTag in _addedTags)
        {
            tags = tags.Add(addedTag);
        }
        try
        {
            ProjectMeta projectMeta = await ProjectManagerService.CreateAsync(new ProjectManagerService.CreateRequestParameters(
                _projectName,
                (ProjectType)selectedProjectTypeIndex,
                _username,
                tags
            ));

            Snackbar.Add($"Project '{_projectName}' created", Severity.Info);
            if (StateService.CognitiveState == null || string.IsNullOrEmpty(StateService.CognitiveState.ProjectId))
            {
                await StateService.SetNetStateAsync(new Web.Shared.Models.UiCognitiveState(projectMeta));
                NavigationManager.NavigateTo($"cognitive/upload/{projectMeta.Id}");
            }
            MudDialog.Close();
        }
        catch (Exception ex)
        {
            Logger.LogWarning((int)EventLogType.UserInteraction, ex, "Could noz create project");
            Snackbar.Add("Could not create project", Severity.Warning);
        }
    }

    private void CloseClicked()
    {
        MudDialog.Close();
    }
}
