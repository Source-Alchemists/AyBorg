using Microsoft.AspNetCore.Components.Web;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class StringInput : BaseInput
{
    private string _value = string.Empty;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _value = (string)Value;
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
