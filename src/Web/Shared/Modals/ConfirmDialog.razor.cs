using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MudBlazor;

namespace AyBorg.Web.Shared.Modals;

public partial class ConfirmDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    [Inject] UserManager<IdentityUser> UserManager { get; set; } = null!;
    [Inject] AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Parameter] public string ContentText { get; set; } = string.Empty;

    [Parameter] public bool NeedPassword { get; set; } = false;

    private string _password = string.Empty;
    private string[] _errors = Array.Empty<string>();

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (firstRender)
        {
            _password = string.Empty;
            _errors = Array.Empty<string>();
        }
    }

    private async void OnConfirmClicked()
    {
        if (NeedPassword)
        {
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            System.Security.Claims.ClaimsPrincipal user = authState.User;
            string userName = user.Identity!.Name!;
            IdentityUser? identity = await UserManager.FindByNameAsync(userName);

            if (!await UserManager.CheckPasswordAsync(identity!, _password))
            {
                _errors = new[] { "Invalid password" };
                return;
            }
        }

        MudDialog.Close(DialogResult.Ok(true));
    }

    private void OnCancelClicked()
    {
        MudDialog.Cancel();
    }
}
