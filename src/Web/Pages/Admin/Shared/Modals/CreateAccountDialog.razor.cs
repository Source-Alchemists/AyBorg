using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using MudBlazor;

namespace AyBorg.Web.Pages.Admin.Shared.Modals;

public partial class CreateAccountDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;

    [Inject] RoleManager<IdentityRole> RoleManager { get; set; } = null!;
    [Inject] UserManager<IdentityUser> UserManager { get; set; } = null!;
    [Inject] IJSRuntime JSRuntime { get; set; } = null!;
    
    private readonly List<Role> _roles = new();
    private string _userName = null!;
    private string _userEmail = null!;
    private string _password = string.Empty;
    private bool _success = false;
    private string[] _errors = Array.Empty<string>();
    private string[] _serviceErrors = Array.Empty<string>();
    private MudForm _form = null!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        RoleManager.Roles.ToList().ForEach(r => _roles.Add(new Role(r)));
        _password = RandomPasswordGenerator.Generate();
    }

    private async void OnCopyToClipboardClicked()
    {
        await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", _password);
    }

    private void OnCancelClicked() => MudDialog.Cancel();

    private async void OnAddClicked()
    {
        await _form.Validate();
        if (!_errors.Any())
        {
            var user = new IdentityUser(_userName)
            {
                Email = _userEmail
            };
            var result = await UserManager.CreateAsync(user, _password);
            if (result.Succeeded)
            {
                foreach(var role in _roles.Where(r => r.Checked))
                {
                    var roleResult = await UserManager.AddToRoleAsync(user, role.IdentityRole.Name!);
                    if(!roleResult.Succeeded)
                    {
                        _serviceErrors = roleResult.Errors.Select(e => e.Description).ToArray();
                        return;
                    }
                }
                _success = true;
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                _serviceErrors = result.Errors.Select(e => e.Description).ToArray();
            }
        }   
    }
}