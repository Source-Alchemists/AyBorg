/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using AyBorg.Authorization;
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
