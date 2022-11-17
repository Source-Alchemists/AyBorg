using AyBorg.SDK.Data.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Projects;

public partial class ConfirmProjectApproveDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    [Inject] UserManager<IdentityUser> UserManager { get; set; } = null!;
    [Inject] AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Parameter] public string BaseUrl { get; set; } = string.Empty;
    [Parameter] public ProjectMetaDto Project { get; set; } = null!;

    private MudForm _form = null!;
    private string _password = string.Empty;
    private string[] _errors = Array.Empty<string>();
    private ProjectMetaDto _tmpProjectMeta = null!;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _tmpProjectMeta = Project with { };
    }

    private void OnCloseClicked()
    {
        MudDialog.Cancel();
    }

    private async void OnValidSubmit()
    {
        await _form.Validate();
        if (_errors.Any())
        {
            _errors = Array.Empty<string>();
            return;
        }
        AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        System.Security.Claims.ClaimsPrincipal user = authState.User;
        string userName = user.Identity!.Name!;
        IdentityUser? identity = await UserManager.FindByNameAsync(userName);

        if (!await UserManager.CheckPasswordAsync(identity!, _password))
        {
            _errors = new[] { "Invalid password" };
            return;
        }
        else
        {
            _tmpProjectMeta.ApprovedBy = userName;
            MudDialog.Close(DialogResult.Ok(_tmpProjectMeta));
        }
    }
}
