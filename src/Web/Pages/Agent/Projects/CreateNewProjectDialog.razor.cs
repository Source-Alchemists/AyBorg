using System.ComponentModel.DataAnnotations;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Shared.Models.Agent;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Projects;

public partial class CreateNewProjectDialog : ComponentBase
{

    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    [Inject] IProjectManagementService ProjectManagementService { get; set; } = null!;
    [Parameter] public string ServiceUniqueName { get; set; } = string.Empty;

    private readonly ProjectMeta _newProject = new() { VersionName = "__DRAFT__" };
    private readonly IList<string> _errors = new List<string>();


    private void OnCloseClicked()
    {
        MudDialog.Cancel();
    }

    private async void OnValidSubmit()
    {
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(_newProject, new ValidationContext(_newProject), validationResults, true))
        {
            validationResults.ForEach(r => _errors.Add(r.ErrorMessage!));
            return;
        }

        ProjectMeta createdProject = await ProjectManagementService.CreateAsync(ServiceUniqueName, _newProject.Name);
        if (createdProject != null)
        {
            MudDialog.Close(DialogResult.Ok(createdProject));
        }
    }
}
