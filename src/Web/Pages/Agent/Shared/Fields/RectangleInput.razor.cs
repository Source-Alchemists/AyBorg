using AyBorg.SDK.Common.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class RectangleInput : BaseInput
{
    [Inject] IDialogService DialogService { get; init; } = null!;

    private Rectangle _value = new();

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _value = (Rectangle)Value;
    }

    private static string ToLabelValue(Rectangle rectangle)
    {
        return $"X = {rectangle.X}, Y = {rectangle.Y}, W = {rectangle.Width}, H = {rectangle.Height}";
    }

    private async Task OnEditClicked()
    {
        var dialogParameters = new DialogParameters
        {
            { "Rectangle", _value }
        };
        IDialogReference dialog = await DialogService.ShowAsync<EditRectangleDialog>("Edit rectangle", dialogParameters);
        DialogResult result = await dialog.Result;

        if (!result.Canceled)
        {
            _value = (Rectangle)result.Data;
            await NotifyValueChangedAsync(_value);
        }
    }
}
