using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace AyBorg.Web.Shared;

public partial class UserMenu : ComponentBase
{
    [CascadingParameter] private Task<AuthenticationState>? AuthenticationState { get; set; }
    [Inject] NavigationManager NavigationManager { get; init; } = null!;
    private string _avatar = string.Empty;
    private string _username = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (AuthenticationState is not null)
        {
            AuthenticationState authState = await AuthenticationState;
            System.Security.Claims.ClaimsPrincipal? user = authState?.User;

            if (user?.Identity is not null)
            {
                _username = user.Identity.Name!;

                _avatar = string.Concat(_username.Where(c => char.IsUpper(c)));
                if (string.IsNullOrEmpty(_avatar))
                {
                    _avatar = _username.First().ToString();
                }

                if (_avatar.Length > 2)
                {
                    _avatar = _avatar.Substring(0, 2);
                }

                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private void Logout()
    {
        NavigationManager.NavigateTo("/Logout", true);
    }
}
