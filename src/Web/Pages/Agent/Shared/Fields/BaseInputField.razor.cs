using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Common.Models;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class BaseInputField : ComponentBase
{
    protected bool _isEditableField = false;

    [Parameter]
    [EditorRequired]
    public Port Port { get; set; } = null!;

    [Parameter] public bool Disabled { get; set; }

    [Parameter] public RenderFragment ChildContent { get; set; } = null!;

    [Parameter] public bool IsEditFieldVisible { get; set; } = false;

    [Parameter] public EventCallback<ValueChangedEventArgs> ValueChanged { get; set; }

    protected override Task OnInitializedAsync()
    {
        if (Port == null)
        {
            throw new ArgumentNullException(nameof(Port));
        }
        _isEditableField = Port.Brand == PortBrand.String
                            || Port.Brand == PortBrand.Numeric
                            || Port.Brand == PortBrand.Rectangle;
        return base.OnInitializedAsync();
    }

    protected virtual async Task NotifyValueChangedAsync(object value)
    {
        await ValueChanged.InvokeAsync(new ValueChangedEventArgs(Port, value));
        IsEditFieldVisible = false;
        await InvokeAsync(StateHasChanged);
    }

    protected virtual void OnEditFieldClicked()
    {
        IsEditFieldVisible = true;
    }
}
