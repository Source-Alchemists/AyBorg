using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Atomy.SDK.Data.DTOs;
using Atomy.Web.Services.Agent;

namespace Atomy.Web.Pages.Agent.Projects;

public partial class CreateNewProjectDialog : ComponentBase
{

    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;

    [Inject] ProjectManagementService _projectManagementService { get; set; } = null!;

    [Parameter] public string BaseUrl { get; set; } = string.Empty;
    private ProjectMetaDto _newProject = new ProjectMetaDto();
    private IList<string> _errors = new List<string>();


    private void OnCloseClicked()
    {
        MudDialog.Cancel();
    }

    private async void OnValidSubmit()
    {
        var validationResults = new List<ValidationResult>();
        if(!Validator.TryValidateObject(_newProject, new System.ComponentModel.DataAnnotations.ValidationContext(_newProject), validationResults, true))
        {
            validationResults.ForEach(r => _errors.Add(r.ErrorMessage!));
            return;
        }

        var createdProject = await _projectManagementService.CreateAsync(BaseUrl, _newProject.Name);
        if (createdProject != null)
        {
            MudDialog.Close(DialogResult.Ok(createdProject));
        }
    }
}