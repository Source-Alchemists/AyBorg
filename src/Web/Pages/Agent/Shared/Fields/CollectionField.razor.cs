using System.Collections.Immutable;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class CollectionField : ComponentBase
{
    private ImmutableList<object> _values = null!;

    [Parameter, EditorRequired] public Port Port { get; set; } = null!;

    [Parameter] public bool Disabled { get; set; }

    [Parameter] public EventCallback<ValueChangedEventArgs> ValueChanged { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        _values = Port.Brand switch
        {
            PortBrand.StringCollection => ImmutableList<object>.Empty.AddRange((ImmutableList<string>)Port.Value!),
            PortBrand.NumericCollection => ConvertNumericCollection(Port.Value!),
            PortBrand.RectangleCollection => ConvertRectangleCollection(Port.Value!),
            _ => throw new InvalidOperationException($"Port {Port.Name} is not a collection."),
        };
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
                {
                    var oldValues = (ImmutableList<string>)port.Value!;
                    return oldValues.Append(null!);
                }
            case PortBrand.NumericCollection:
                {
                    var oldValues = (ImmutableList<double>)port.Value!;
                    return oldValues.Append(0);
                }
            case PortBrand.RectangleCollection:
                {
                    var oldValues = (ImmutableList<Rectangle>)port.Value!;
                    return oldValues.Append(new Rectangle());
                }
            default:
                throw new InvalidOperationException($"Port {port.Name} is not a collection.");
        }
    }

    private static object RemoveItemAt(Port port, int index)
    {
        switch (port.Brand)
        {
            case PortBrand.StringCollection:
                {
                    var oldValues = (ImmutableList<string>)port.Value!;
                    return oldValues.RemoveAt(index);
                }
            case PortBrand.NumericCollection:
                {
                    var oldValues = (ImmutableList<double>)port.Value!;
                    return oldValues.RemoveAt(index);
                }
            case PortBrand.RectangleCollection:
                {
                    var oldValues = (ImmutableList<Rectangle>)port.Value!;
                    return oldValues.RemoveAt(index);
                }

            default:
                throw new InvalidOperationException($"Port {port.Name} is not a collection.");

        }
    }

    private static object CreateBrandType(Port port, ImmutableList<object> oldValues, BaseInput.InputChange change)
    {
        switch (port.Brand)
        {
            case PortBrand.StringCollection:
                {
                    string[] newValues = oldValues.Cast<string>().ToArray();
                    newValues[change.Index] = (string)change.Value;
                    return newValues.ToImmutableList();
                }
            case PortBrand.NumericCollection:
                {
                    double[] newValues = oldValues.Cast<double>().ToArray();
                    newValues[change.Index] = (double)change.Value;
                    return newValues.ToImmutableList();
                }
            case PortBrand.RectangleCollection:
                {
                    Rectangle[] newValues = oldValues.Cast<Rectangle>().ToArray();
                    newValues[change.Index] = (Rectangle)change.Value;
                    return newValues.ToImmutableList();
                }
            default:
                throw new InvalidOperationException($"Port {port.Name} is not a collection.");
        }
    }

    private static ImmutableList<object> ConvertNumericCollection(object value)
    {
        var tmpCollection = (ImmutableList<double>)value;
        ImmutableList<object> resultList = ImmutableList<object>.Empty;
        foreach (double tmp in tmpCollection)
        {
            resultList.Add(tmp);
        }

        return resultList;
    }

    private static ImmutableList<object> ConvertRectangleCollection(object value)
    {
        var tmpCollection = (ImmutableList<Rectangle>)value;
        ImmutableList<object> resultList = ImmutableList<object>.Empty;
        foreach (Rectangle tmp in tmpCollection)
        {
            resultList.Add(tmp);
        }

        return resultList;
    }
}
