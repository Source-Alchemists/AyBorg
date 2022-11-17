using System.Text.Json;
using Microsoft.AspNetCore.Components.Web;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class NumericInputField : BaseInputField
{
    private double _value = 0;

    protected override Task OnInitializedAsync()
    {
        base.OnInitializedAsync();
        if (Port.Value == null) throw new ArgumentNullException(nameof(Port.Value));
        if(Port.Value is JsonElement jsonElement)
        {
            _value = JsonSerializer.Deserialize<double>(jsonElement)!;
        }
        else if(Port.Value is double value)
        {
            _value = value;
        }
        Port.Value = _value;
        return Task.CompletedTask;
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