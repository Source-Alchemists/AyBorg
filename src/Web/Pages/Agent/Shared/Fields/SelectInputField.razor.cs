using AyBorg.SDK.Common.Ports;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

#nullable disable

public partial class SelectInputField : BaseInputField
{
    private SelectPort.ValueContainer _value;
    private readonly IList<string> _selectedValues = new List<string>();
    private string[] _values = Array.Empty<string>();

    protected override void OnParametersSet()
    {
        _selectedValues.Clear();
        if(Port.Value is SelectPort.ValueContainer value)
        {
            _value = value;
            _selectedValues.Add(_value.SelectedValue ?? string.Empty);
            _values = _value.Values.ToArray();
        }

        base.OnParametersSet();
    }

    private async void OnSelectedValuesChanged(IEnumerable<string> values)
    {
        _value = _value with { SelectedValue = values.FirstOrDefault() };
        await NotifyValueChangedAsync(_value);
    }
}
