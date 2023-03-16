using AyBorg.SDK.Common.Ports;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class BaseInput : ComponentBase
{
    [Parameter, EditorRequired] public object Value { get; set; } = null!;
    [Parameter, EditorRequired] public PortBrand PortBrand { get; init; }

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public int CollectionIndex { get; set; }

    [Parameter] public EventCallback<ValueChangedEventArgs> ValueChanged { get; set; }

    protected virtual async Task NotifyValueChangedAsync(object value)
    {
        await ValueChanged.InvokeAsync(new ValueChangedEventArgs(null!, new InputChange(value, CollectionIndex)));
    }

    public record InputChange(object Value, int Index);
}
