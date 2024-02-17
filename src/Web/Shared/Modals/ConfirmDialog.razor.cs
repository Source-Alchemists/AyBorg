using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MudBlazor;

namespace AyBorg.Web.Shared.Modals;

public partial class ConfirmDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; init; } = null!;
    [Inject] UserManager<IdentityUser> UserManager { get; init; } = null!;
    [Inject] AuthenticationStateProvider AuthenticationStateProvider { get; init; } = null!;

    [Parameter] public string ContentText { get; init; } = string.Empty;
    [Parameter] public bool NeedPassword { get; init; } = false;
    [Parameter] public bool ShowComment { get; init; } = false;
    [Parameter] public string Comment { get; init; } = string.Empty;
    [Parameter] public bool ShowVersion { get; init; } = false;
    [Parameter] public string Version { get; init; } = string.Empty;

    private MudForm _form = null!;
    private string _password = string.Empty;
    private string _comment = string.Empty;
    private string _version = string.Empty;
    private string[] _errors = Array.Empty<string>();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            _comment = Comment;
            _version = Version;
            _password = string.Empty;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void OnConfirmClicked()
    {
        await _form.Validate();
        string userName = string.Empty;

        if (_errors.Any())
        {
            _errors = Array.Empty<string>();
            return;
        }

        if (NeedPassword)
        {
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            System.Security.Claims.ClaimsPrincipal user = authState.User;
            userName = user.Identity!.Name!;
            IdentityUser? identity = await UserManager.FindByNameAsync(userName);

            if (!await UserManager.CheckPasswordAsync(identity!, _password))
            {
                _errors = new[] { "Invalid password" };
                return;
            }
        }

        MudDialog.Close(DialogResult.Ok(new ConfirmResult(_comment, _version, userName)));
    }

    private void OnCancelClicked()
    {
        MudDialog.Cancel();
    }

    public record ConfirmResult(string Comment, string Version, string UserName);
}
