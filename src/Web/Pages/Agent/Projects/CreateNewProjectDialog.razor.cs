using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Autodroid.SDK.Data.DTOs;
using Autodroid.Web.Services.Agent;

namespace Autodroid.Web.Pages.Agent.Projects;

public partial class CreateNewProjectDialog : ComponentBase
{

    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;

    [Inject] ProjectManagementService ProjectManagementService { get; set; } = null!;

    [Parameter] public string BaseUrl { get; set; } = string.Empty;
    private readonly ProjectMetaDto _newProject = new();
    private readonly IList<string> _errors = new List<string>();


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

        var createdProject = await ProjectManagementService.CreateAsync(BaseUrl, _newProject.Name);
        if (createdProject != null)
        {
            MudDialog.Close(DialogResult.Ok(createdProject));
        }
    }
}