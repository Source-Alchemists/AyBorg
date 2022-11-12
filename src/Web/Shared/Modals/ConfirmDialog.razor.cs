using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Shared.Modals;

public partial class ConfirmDialog : ComponentBase
{
	[CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public string ContentText { get; set; } = string.Empty;

    [Parameter]
	public EventCallback<DialogResult> Closed { get; set; }

	private void OnConfirmClicked()
	{
		MudDialog.Close(DialogResult.Ok(true));
	}

	private void OnCancelClicked()
	{
		MudDialog.Cancel();
	}
}