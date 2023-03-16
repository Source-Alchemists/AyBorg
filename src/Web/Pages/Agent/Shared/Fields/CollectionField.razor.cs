using System.Collections.ObjectModel;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class CollectionField : ComponentBase
{
    private List<object> _values = null!;

    [Parameter, EditorRequired] public Port Port { get; set; } = null!;

    [Parameter] public bool Disabled { get; set; }

    [Parameter] public EventCallback<ValueChangedEventArgs> ValueChanged { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        switch (Port.Brand)
        {
            case PortBrand.StringCollection:
                _values = new List<object>((ReadOnlyCollection<string>)Port.Value!).Cast<object>().ToList();
                break;
            default:
                throw new InvalidOperationException($"Port {Port.Name} is not a collection.");

        }
    }

    private async Task AddItemClicked()
    {
        object newCollection = AddEmptyItemTo(Port);
        await ValueChanged.InvokeAsync(new ValueChangedEventArgs(Port, newCollection));
    }

    private async Task RemoveItemClicked(int index)
    {
        _values.RemoveAt(index);
        object newCollection = RemoveItemAt(Port, index);
        await ValueChanged.InvokeAsync(new ValueChangedEventArgs(Port, newCollection));
    }

    private async Task InputValueChanged(ValueChangedEventArgs args)
    {
        object brandType = CreateBrandType(Port, _values, (BaseInput.InputChange)args.Value!);
        await ValueChanged.InvokeAsync(new ValueChangedEventArgs(Port, brandType));
    }

    private static object AddEmptyItemTo(Port port)
    {
        switch (port.Brand)
        {
            case PortBrand.StringCollection:
                var oldValues = (ReadOnlyCollection<string>)port.Value!;
                return new ReadOnlyCollection<string>(oldValues.Append(null!).ToList());
            default:
                throw new InvalidOperationException($"Port {port.Name} is not a collection.");
        }
    }

    private static object RemoveItemAt(Port port, int index)
    {
        switch (port.Brand)
        {
            case PortBrand.StringCollection:
                var oldValues = new List<string>((ReadOnlyCollection<string>)port.Value!);
                var oldList = oldValues.ToList();
                oldList.RemoveAt(index);
                return new ReadOnlyCollection<string>(oldList);
            default:
                throw new InvalidOperationException($"Port {port.Name} is not a collection.");

        }
    }

    private static object CreateBrandType(Port port, List<object> oldValues, BaseInput.InputChange change)
    {
        switch (port.Brand)
        {
            case PortBrand.StringCollection:
                string[] newValues = oldValues.Cast<string>().ToArray();
                newValues[change.Index] = (string)change.Value;
                return new ReadOnlyCollection<string>(newValues);
            default:
                throw new InvalidOperationException($"Port {port.Name} is not a collection.");
        }
    }
}
