using Microsoft.AspNetCore.Identity;

namespace AyBorg.Web.Pages.Admin.Shared.Modals;
internal record Role
{
    public readonly IdentityRole IdentityRole;
    public bool Checked = false;
    public bool Disabled = false;
    
    public Role(IdentityRole role)
    {
        IdentityRole = role;
    }
}