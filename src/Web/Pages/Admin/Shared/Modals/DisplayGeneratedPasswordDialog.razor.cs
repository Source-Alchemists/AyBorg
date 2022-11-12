using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace AyBorg.Web.Pages.Admin.Shared.Modals;

public partial class DisplayGeneratedPasswordDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public string Password { get; set; } = null!;
    [Inject] IJSRuntime JSRuntime { get; set; } = null!;
    
    private async void OnCopyToClipboardClicked()
    {
        await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", Password);
    }

    private void OnCloseClicked() => MudDialog.Close(DialogResult.Ok(true));
}