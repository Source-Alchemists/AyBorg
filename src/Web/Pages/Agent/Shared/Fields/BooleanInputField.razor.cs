namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class BooleanInputField : BaseInputField
{
    private bool _value = false;

    protected override Task OnParametersSetAsync()
    {
        if (Port.Value == null) throw new ArgumentNullException(nameof(Port.Value));
        if (Port.Value is bool value)
        {
            _value = value;
        }
        return base.OnParametersSetAsync();
    }

    private async void OnCheckChanged(bool value)
    {
        _value = value;
        await NotifyValueChangedAsync(_value);
    }
}
