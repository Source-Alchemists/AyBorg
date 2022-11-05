using System.Text.Json;
using Atomy.SDK.Data.DTOs;

namespace Atomy.Web.Pages.Agent.Shared.Fields;

public partial class EnumInputField : BaseInputField
{
    private EnumDto _value = new();
    private readonly IList<string> _selectedNames = new List<string>();
    private string[] _names = Array.Empty<string>();

    protected override Task OnParametersSetAsync()
    {
        _selectedNames.Clear();
        if (Port.Value == null)
        {
            _value = new EnumDto();
        }
        else if (Port.Value is EnumDto value)
        {
            _value = value;
        }
        else
        {
            var valueKind = (JsonElement)Port.Value;
            _value = JsonSerializer.Deserialize<EnumDto>(valueKind, new JsonSerializerOptions {  PropertyNameCaseInsensitive = true })!;
            Port.Value = _value;
        }

        _selectedNames.Add(_value.Name ?? string.Empty);
        _names = _value.Names ?? Array.Empty<string>();
        return base.OnParametersSetAsync();
    }

    private async void OnSelectedValuesChanged(IEnumerable<string> values)
    {
        _value.Name = values.FirstOrDefault();
        await NotifyValueChangedAsync(_value);
    }
}