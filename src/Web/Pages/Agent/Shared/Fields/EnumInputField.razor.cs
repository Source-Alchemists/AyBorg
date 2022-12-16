using System.Text.Json;
namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class EnumInputField : BaseInputField
{
    private SDK.Data.Bindings.Enum _value = new();
    private readonly IList<string> _selectedNames = new List<string>();
    private string[] _names = Array.Empty<string>();

    protected override Task OnParametersSetAsync()
    {
        _selectedNames.Clear();
        if (Port.Value == null) throw new ArgumentNullException(nameof(Port.Value));
        else if (Port.Value is SDK.Data.Bindings.Enum value)
        {
            _value = value;
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
