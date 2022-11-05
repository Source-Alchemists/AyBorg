using Atomy.Web.Pages.Admin.Shared.Modals;
using Atomy.Web.Shared.Modals;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using MudBlazor;

namespace Atomy.Web.Pages.Admin;

public partial class UserManagement : ComponentBase
{
    [Inject] UserManager<IdentityUser> UserManager { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;
    [Inject] ISnackbar Snackbar { get; set; } = null!;

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
        var dialog = DialogService.Show<CreateAccountDialog>("Add Account", dialogParameters, dialogOptions);
        var result = await dialog.Result;
        if (!result.Cancelled)
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
        var dialog = DialogService.Show<EditAccountDialog>("Edit Account", dialogParameters, dialogOptions);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            _users = UserManager.Users.ToList();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void OnLockAccountClicked(IdentityUser listUser)
    {
        var user = await FindActualUserAsync(listUser);
        var dialog = DialogService.Show<ConfirmDialog>("Lock Account", new DialogParameters { { "ContentText", $"Are you sure you want to lock '{user.UserName}'?" } });
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            user.LockoutEnd = DateTime.Now.AddYears(1000);
            await UserManager.UpdateAsync(user);
            _users = UserManager.Users.ToList();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void OnUnlockAccountClicked(IdentityUser listUser)
    {
        var user = await FindActualUserAsync(listUser);
        var dialog = DialogService.Show<ConfirmDialog>("Unlock Account", new DialogParameters { { "ContentText", $"Are you sure you want to unlock '{user.UserName}'?" } });
        var result = await dialog.Result;
        if (!result.Cancelled)
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
        var user = await FindActualUserAsync(listUser);
        var dialog = DialogService.Show<ConfirmDialog>("Reset Password", new DialogParameters { { "ContentText", $"Are you sure you want to reset the password for '{user.UserName}'?" } });
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            var newPassword = RandomPasswordGenerator.Generate();
            var pwResult = await UserManager.RemovePasswordAsync(user);
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

            dialog = DialogService.Show<DisplayGeneratedPasswordDialog>("New Password", new DialogParameters { { "Password", newPassword } }, 
                                        new DialogOptions { CloseButton = true, CloseOnEscapeKey = false, DisableBackdropClick = true });
            await dialog.Result;

            _users = UserManager.Users.ToList();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task<IdentityUser> FindActualUserAsync(IdentityUser user)
    {
        // This method is needed because the user could be changed in the background or from a other admin page.
        return await UserManager.FindByIdAsync(user.Id);
    }
}