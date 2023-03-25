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

        _values = Port.Brand switch
        {
            PortBrand.StringCollection => new List<object>((ReadOnlyCollection<string>)Port.Value!),
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
                    var oldValues = (ReadOnlyCollection<string>)port.Value!;
                    return new ReadOnlyCollection<string>(oldValues.Append(null!).ToList());
                }
            case PortBrand.NumericCollection:
                {
                    var oldValues = (ReadOnlyCollection<double>)port.Value!;
                    return new ReadOnlyCollection<double>(oldValues.Append(0).ToList());
                }
            case PortBrand.RectangleCollection:
                {
                    var oldValues = (ReadOnlyCollection<Rectangle>)port.Value!;
                    return new ReadOnlyCollection<Rectangle>(oldValues.Append(new Rectangle()).ToList());
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
                    var oldValues = new List<string>((ReadOnlyCollection<string>)port.Value!);
                    var oldList = oldValues.ToList();
                    oldList.RemoveAt(index);
                    return new ReadOnlyCollection<string>(oldList);
                }
            case PortBrand.NumericCollection:
                {
                    var oldValues = new List<double>((ReadOnlyCollection<double>)port.Value!);
                    var oldList = oldValues.ToList();
                    oldList.RemoveAt(index);
                    return new ReadOnlyCollection<double>(oldList);
                }
            case PortBrand.RectangleCollection:
                {
                    var oldValues = new List<Rectangle>((ReadOnlyCollection<Rectangle>)port.Value!);
                    var oldList = oldValues.ToList();
                    oldList.RemoveAt(index);
                    return new ReadOnlyCollection<Rectangle>(oldList);
                }

            default:
                throw new InvalidOperationException($"Port {port.Name} is not a collection.");

        }
    }

    private static object CreateBrandType(Port port, List<object> oldValues, BaseInput.InputChange change)
    {
        switch (port.Brand)
        {
            case PortBrand.StringCollection:
                {
                    string[] newValues = oldValues.Cast<string>().ToArray();
                    newValues[change.Index] = (string)change.Value;
                    return new ReadOnlyCollection<string>(newValues);
                }
            case PortBrand.NumericCollection:
                {
                    double[] newValues = oldValues.Cast<double>().ToArray();
                    newValues[change.Index] = (double)change.Value;
                    return new ReadOnlyCollection<double>(newValues);
                }
            case PortBrand.RectangleCollection:
                {
                    Rectangle[] newValues = oldValues.Cast<Rectangle>().ToArray();
                    newValues[change.Index] = (Rectangle)change.Value;
                    return new ReadOnlyCollection<Rectangle>(newValues);
                }
            default:
                throw new InvalidOperationException($"Port {port.Name} is not a collection.");
        }
    }

    private static List<object> ConvertNumericCollection(object value)
    {
        var tmpCollection = (ReadOnlyCollection<double>)value;
        var resultList = new List<object>();
        foreach (double tmp in tmpCollection)
        {
            resultList.Add(tmp);
        }

        return resultList;
    }

    private static List<object> ConvertRectangleCollection(object value)
    {
        var tmpCollection = (ReadOnlyCollection<Rectangle>)value;
        var resultList = new List<object>();
        foreach (Rectangle tmp in tmpCollection)
        {
            resultList.Add(tmp);
        }

        return resultList;
    }
}
