using AyBorg.SDK.Authorization;
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
