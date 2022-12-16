using Microsoft.AspNetCore.Components.Web;
using AyBorg.SDK.Data.Bindings;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class RectangleInputField : BaseInputField
{
    private Rectangle _value = new();
    private bool _isXEditing = false;
    private bool _isYEditing = false;
    private bool _isWidthEditing = false;
    private bool _isHeightEditing = false;

    protected override Task OnParametersSetAsync()
    {
        if (Port.Value == null) return Task.CompletedTask;
        if (Port.Value is Rectangle value)
        {
            _value = value;
            return Task.CompletedTask;
        }
        Port.Value = _value;
        return base.OnParametersSetAsync();
    }

    private async void OnKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await NotifyValueChangedAsync(_value);
            ResetButtons();
        }
    }

    private async void OnFocusOut(FocusEventArgs e)
    {
        await NotifyValueChangedAsync(_value);
        ResetButtons();
    }

    private void OnXClicked()
    {
        _isXEditing = true;
    }

    private void OnYClicked()
    {
        _isYEditing = true;
    }

    private void OnWidthClicked()
    {
        _isWidthEditing = true;
    }

    private void OnHeightClicked()
    {
        _isHeightEditing = true;
    }

    private void ResetButtons()
    {
        _isXEditing = false;
        _isYEditing = false;
        _isWidthEditing = false;
        _isHeightEditing = false;
        StateHasChanged();
    }
}
