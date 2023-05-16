using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class EditFieldButton : ComponentBase
{
    [Parameter, EditorRequired] public string Label { get; set; } = string.Empty;
    [Parameter, EditorRequired] public object Value { get; set; } = new object();
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public EventCallback Clicked { get; set; }

    private async void OnEditFiedClicked()
    {
        await Clicked.InvokeAsync();
    }
}
