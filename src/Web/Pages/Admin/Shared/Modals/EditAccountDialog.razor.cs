using Atomy.SDK.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using MudBlazor;

namespace Atomy.Web.Pages.Admin.Shared.Modals;

public partial class EditAccountDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public IdentityUser User { get; set; } = null!;
    [Inject] RoleManager<IdentityRole> RoleManager { get; set; } = null!;
    [Inject] UserManager<IdentityUser> UserManager { get; set; } = null!;

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
                Checked = await UserManager.IsInRoleAsync(User, r.Name)
            };
            if (r.Name == Roles.Administrator && User.UserName == "SystemAdmin")
            {
                role.Disabled = true;
            }
            _roles.Add(role);
        });
        _userEmail = User.Email;
    }

    private void OnCancelClicked() => MudDialog.Cancel();

    private async void OnApplyClicked()
    {
        await _form.Validate();
        if (!_errors.Any())
        {
            var user = await UserManager.FindByIdAsync(User.Id);
            user.Email = _userEmail;
            var result = await UserManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _errors = result.Errors.Select(e => e.Description).ToArray();
                return;
            }

            foreach (var role in _roles)
            {
                if (role.Checked && !await UserManager.IsInRoleAsync(user, role.IdentityRole.Name))
                {
                    var roleResult = await UserManager.AddToRoleAsync(user, role.IdentityRole.Name);
                    if (!roleResult.Succeeded)
                    {
                        _errors = roleResult.Errors.Select(e => e.Description).ToArray();
                        return;
                    }
                }
                else if (!role.Checked && await UserManager.IsInRoleAsync(user, role.IdentityRole.Name))
                {
                    var roleResult = await UserManager.RemoveFromRoleAsync(user, role.IdentityRole.Name);
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