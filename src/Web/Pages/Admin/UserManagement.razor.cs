using AyBorg.Web.Pages.Admin.Shared;
using AyBorg.Web.Pages.Admin.Shared.Modals;
using AyBorg.Web.Shared.Modals;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using MudBlazor;

namespace AyBorg.Web.Pages.Admin;

public partial class UserManagement : ComponentBase
{
    [Inject] UserManager<IdentityUser> UserManager { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;
    [Inject] ISnackbar Snackbar { get; set; } = null!;
    [Inject] ILogger<UserManagement> Logger { get; set; } = null!;

    private List<IdentityUser> _users = null!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _users = UserManager.Users.ToList();
    }

    private async Task OnAddAccountClicked()
    {
        var dialogOptions = new DialogOptions();
        var dialogParameters = new DialogParameters();
        IDialogReference dialog = await DialogService.ShowAsync<CreateAccountDialog>("Add Account", dialogParameters, dialogOptions);
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            _users = UserManager.Users.ToList();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnEditAccountClicked(IdentityUser listUser)
    {
        var dialogOptions = new DialogOptions();
        var dialogParameters = new DialogParameters
        {
            { "User", listUser }
        };
        IDialogReference dialog = await DialogService.ShowAsync<EditAccountDialog>("Edit Account", dialogParameters, dialogOptions);
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            _users = UserManager.Users.ToList();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void OnLockAccountClicked(IdentityUser listUser)
    {
        IdentityUser user = await FindActualUserAsync(listUser);
        IDialogReference dialog = await DialogService.ShowAsync<ConfirmDialog>("Lock Account", new DialogParameters { { "ContentText", $"Are you sure you want to lock '{user.UserName}'?" } });
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            user.LockoutEnd = DateTime.Now.AddYears(1000);
            await UserManager.UpdateAsync(user);
            _users = UserManager.Users.ToList();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void OnUnlockAccountClicked(IdentityUser listUser)
    {
        IdentityUser user = await FindActualUserAsync(listUser);
        IDialogReference dialog = await DialogService.ShowAsync<ConfirmDialog>("Unlock Account", new DialogParameters { { "ContentText", $"Are you sure you want to unlock '{user.UserName}'?" } });
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            user.LockoutEnd = null;
            user.AccessFailedCount = 0;
            await UserManager.UpdateAsync(user);
            _users = UserManager.Users.ToList();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void OnResetPasswordClicked(IdentityUser listUser)
    {
        IdentityUser user = await FindActualUserAsync(listUser);
        IDialogReference dialog = await DialogService.ShowAsync<ConfirmDialog>("Reset Password", new DialogParameters { { "ContentText", $"Are you sure you want to reset the password for '{user.UserName}'?" } });
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            string newPassword = RandomPasswordGenerator.Generate();
            IdentityResult pwResult = await UserManager.RemovePasswordAsync(user);
            if (!pwResult.Succeeded)
            {
                Snackbar.Add(pwResult.Errors.First().Description, Severity.Error);
                return;
            }
            pwResult = await UserManager.AddPasswordAsync(user, newPassword);
            if (!pwResult.Succeeded)
            {
                Snackbar.Add(pwResult.Errors.First().Description, Severity.Error);
                return;
            }

            dialog = await DialogService.ShowAsync<DisplayGeneratedPasswordDialog>("New Password", new DialogParameters { { "Password", newPassword } },
                                        new DialogOptions { CloseButton = true, CloseOnEscapeKey = false, DisableBackdropClick = true });
            await dialog.Result;

            _users = UserManager.Users.ToList();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task<IdentityUser> FindActualUserAsync(IdentityUser user)
    {
        // This method is needed because the user could be changed in the background or from a other admin page.
        IdentityUser? identity = await UserManager.FindByIdAsync(user.Id);
        if (identity == null)
        {
            Logger.LogWarning("User with id '{Id}' not found.", user.Id);
            Snackbar.Add("User not found.", Severity.Error);
            return null!;
        }

        return identity;
    }
}
