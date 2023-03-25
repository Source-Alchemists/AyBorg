using AyBorg.SDK.Common.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Shared;

public partial class EditRectangleDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public Rectangle Rectangle { get; init; }

    private Rectangle _rectangle;

    protected override void OnParametersSet() {
        base.OnParametersSet();
        _rectangle = Rectangle;
    }

    private void OnCancelClicked() => MudDialog.Cancel();

    private void OnApplyClicked() => MudDialog.Close(DialogResult.Ok(_rectangle));

    public record struct Parameters (Rectangle Rectangle, int Index);
}
