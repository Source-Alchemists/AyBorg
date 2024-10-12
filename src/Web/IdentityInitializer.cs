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

using AyBorg.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AyBorg.Web;

internal static class IdentityInitializer
{
    public static async ValueTask InitializeAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        await CreateRoleAsync(roleManager, Roles.Administrator);
        await CreateRoleAsync(roleManager, Roles.Engineer);
        await CreateRoleAsync(roleManager, Roles.Auditor);
        await CreateRoleAsync(roleManager, Roles.Reviewer);

        const string defaultAdminUser = "SystemAdmin";
        if(!userManager.Users.Any(u => u.UserName == defaultAdminUser))
        {
            IdentityResult userResult = await userManager.CreateAsync(new IdentityUser(defaultAdminUser), "SystemAdmin123!");
            if(!userResult.Succeeded)
            {
                throw new Exception("Failed to create administrator user");
            }

            await userManager.FindByNameAsync(defaultAdminUser).ContinueWith(task =>
            {
                IdentityUser? user = task.Result;
                userManager.AddToRoleAsync(user!, "Administrator");
                user!.EmailConfirmed = true;
                user.Email = "systemadmin@ayborg.io";
                user.LockoutEnabled = false;
                userManager.UpdateAsync(user);
            });
        }
    }

    private static async ValueTask CreateRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if(!await roleManager.RoleExistsAsync(roleName))
        {
            IdentityResult roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            if(!roleResult.Succeeded)
            {
                throw new Exception($"Failed to create '{roleName}' role");
            }
        }
    }
}
