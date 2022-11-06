using System.Text.Json;

namespace Autodroid.Web.Pages.Agent.Shared.Fields;

public partial class BooleanInputField : BaseInputField
{
    private bool _value = false;

    protected override Task OnParametersSetAsync()
    {
        if (Port.Value == null)
        {
            _value = false;
        }
        else if (Port.Value is bool value)
        {
            _value = value;
        }
        else
        {
            var valueKind = (JsonElement)Port.Value;
            _value = JsonSerializer.Deserialize<bool>(valueKind);
            Port.Value = _value;
        }
        return base.OnParametersSetAsync();
    }

    private async void OnCheckChanged(bool value)
    {
        _value = value;
        await NotifyValueChangedAsync(_value);
    }
}