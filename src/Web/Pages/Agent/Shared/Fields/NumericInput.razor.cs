using Microsoft.AspNetCore.Components.Web;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class NumericInput : BaseInput
{
    private double _value = 0d;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _value = (double)Value;
    }

    private async void OnKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await NotifyValueChangedAsync(_value);
        }
    }

    private async void OnFocusOut(FocusEventArgs e)
    {
        await NotifyValueChangedAsync(_value);
    }
}
