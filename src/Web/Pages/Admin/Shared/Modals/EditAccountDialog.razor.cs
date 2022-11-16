using AyBorg.SDK.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using MudBlazor;

namespace AyBorg.Web.Pages.Admin.Shared.Modals;

public partial class EditAccountDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public IdentityUser User { get; set; } = null!;
    [Inject] RoleManager<IdentityRole> RoleManager { get; set; } = null!;
    [Inject] UserManager<IdentityUser> UserManager { get; set; } = null!;
    [Inject] ILogger<EditAccountDialog> Logger { get; set; } = null!;

    private readonly List<Role> _roles = new();
    private MudForm _form = null!;
    private string _userEmail = string.Empty;
    private bool _success = false;
    private string[] _errors = Array.Empty<string>();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        RoleManager.Roles.ToList().ForEach(async r =>
        {
            var role = new Role(r)
            {
                Checked = await UserManager.IsInRoleAsync(User, r.Name!)
            };
            if (r.Name == Roles.Administrator && User.UserName == "SystemAdmin")
            {
                role.Disabled = true;
            }
            _roles.Add(role);
        });
        _userEmail = User.Email ?? string.Empty;
    }

    private void OnCancelClicked() => MudDialog.Cancel();

    private async void OnApplyClicked()
    {
        await _form.Validate();
        if (!_errors.Any())
        {
            IdentityUser? user = await UserManager.FindByIdAsync(User.Id);
            if(user == null)
            {
                Logger.LogWarning("User not found");
                return;
            }

            user.Email = _userEmail;
            IdentityResult result = await UserManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _errors = result.Errors.Select(e => e.Description).ToArray();
                return;
            }

            foreach (Role role in _roles)
            {
                if (role.Checked && !await UserManager.IsInRoleAsync(user, role.IdentityRole.Name!))
                {
                    IdentityResult roleResult = await UserManager.AddToRoleAsync(user, role.IdentityRole.Name!);
                    if (!roleResult.Succeeded)
                    {
                        _errors = roleResult.Errors.Select(e => e.Description).ToArray();
                        return;
                    }
                }
                else if (!role.Checked && await UserManager.IsInRoleAsync(user, role.IdentityRole.Name!))
                {
                    IdentityResult roleResult = await UserManager.RemoveFromRoleAsync(user, role.IdentityRole.Name!);
                    if (!roleResult.Succeeded)
                    {
                        _errors = roleResult.Errors.Select(e => e.Description).ToArray();
                        return;
                    }
                }

                _success = true;
            }

            MudDialog.Close(DialogResult.Ok(true));
        }
    }
}
