using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class EditFieldButton : ComponentBase
{
    [Parameter, EditorRequired] public string Label { get; set; } = string.Empty;
    [Parameter, EditorRequired] public object Value { get; set; } = new object();
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public EventCallback Clicked { get; set; }

    [Inject] IJSRuntime JSRuntime { get; init; } = null!;

    private ElementReference _containerRef;
    private string _valueStyle = string.Empty;
    private string _tooltipStyle = string.Empty;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        BoundingClientRect rectangle = await JSRuntime.InvokeAsync<BoundingClientRect>("getElementBoundingClientRect", _containerRef);
        double width = double.Round(rectangle.Width);
        _valueStyle = $"width: {width}px; max-width: {width}px; max-height: 30px";
        _tooltipStyle = $"width: {width}px";
    }

    private async void OnEditFiedClicked()
    {
        await Clicked.InvokeAsync();
    }
}
