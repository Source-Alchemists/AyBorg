using Autodroid.SDK.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Autodroid.Web;

internal static class IdentityInitializer
{
    public static async Task InitializeAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        await CreateRoleAsync(roleManager, Roles.Administrator);
        await CreateRoleAsync(roleManager, Roles.Engineer);
        await CreateRoleAsync(roleManager, Roles.Auditor);

        const string defaultAdminUser = "SystemAdmin";
        if(!userManager.Users.Any(u => u.UserName == defaultAdminUser))
        {
            var userResult = await userManager.CreateAsync(new IdentityUser(defaultAdminUser), "SystemAdmin123!");
            if(!userResult.Succeeded)
            {
                throw new Exception("Failed to create administrator user");
            }
            
            await userManager.FindByNameAsync(defaultAdminUser).ContinueWith(task =>
            {
                var user = task.Result;
                userManager.AddToRoleAsync(user, "Administrator");
                user.EmailConfirmed = true;
                user.LockoutEnabled = false;
                userManager.UpdateAsync(user);
            });
        }
    }

    private static async Task CreateRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if(!await roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            if(!roleResult.Succeeded)
            {
                throw new Exception($"Failed to create '{roleName}' role");
            }
        }
    }
}