/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Immutable;
using AyBorg.Types.Models;
using AyBorg.Types.Ports;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class CollectionField : ComponentBase
{
    private ImmutableList<object> _values = null!;

    [Parameter, EditorRequired] public PortModel Port { get; set; } = null!;

    [Parameter] public bool Disabled { get; set; }

    [Parameter] public EventCallback<ValueChangedEventArgs> ValueChanged { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        _values = Port.Brand switch
        {
            PortBrand.StringCollection => ImmutableList<object>.Empty.AddRange((IEnumerable<string>)Port.Value!),
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
        _values = _values.RemoveAt(index);
        object newCollection = RemoveItemAt(Port, index);
        await ValueChanged.InvokeAsync(new ValueChangedEventArgs(Port, newCollection));
    }

    private async Task InputValueChanged(ValueChangedEventArgs args)
    {
        object brandType = CreateBrandType(Port, _values, (BaseInput.InputChange)args.Value!);
        await ValueChanged.InvokeAsync(new ValueChangedEventArgs(Port, brandType));
    }

    private static object AddEmptyItemTo(PortModel port)
    {
        switch (port.Brand)
        {
            case PortBrand.StringCollection:
                {
                    var oldValues = (IEnumerable<string>)port.Value!;
                    return oldValues.ToImmutableList().Append(null!);
                }
            case PortBrand.NumericCollection:
                {
                    var oldValues = (IEnumerable<double>)port.Value!;
                    return oldValues.ToImmutableList().Append(0);
                }
            case PortBrand.RectangleCollection:
                {
                    var oldValues = (IEnumerable<RectangleModel>)port.Value!;
                    return oldValues.ToImmutableList().Append(new RectangleModel());
                }
            default:
                throw new InvalidOperationException($"Port {port.Name} is not a collection.");
        }
    }

    private static object RemoveItemAt(PortModel port, int index)
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
                    var oldValues = (ImmutableList<RectangleModel>)port.Value!;
                    return oldValues.RemoveAt(index);
                }

            default:
                throw new InvalidOperationException($"Port {port.Name} is not a collection.");

        }
    }

    private static object CreateBrandType(PortModel port, ImmutableList<object> oldValues, BaseInput.InputChange change)
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
                    RectangleModel[] newValues = oldValues.Cast<RectangleModel>().ToArray();
                    newValues[change.Index] = (RectangleModel)change.Value;
                    return newValues.ToImmutableList();
                }
            default:
                throw new InvalidOperationException($"Port {port.Name} is not a collection.");
        }
    }

    private static ImmutableList<object> ConvertNumericCollection(object value)
    {
        var tmpCollection = (IEnumerable<double>)value;
        ImmutableList<object> resultList = ImmutableList<object>.Empty;
        foreach (double tmp in tmpCollection)
        {
            resultList = resultList.Add(tmp);
        }

        return resultList;
    }

    private static ImmutableList<object> ConvertRectangleCollection(object value)
    {
        var tmpCollection = (IEnumerable<RectangleModel>)value;
        ImmutableList<object> resultList = ImmutableList<object>.Empty;
        foreach (RectangleModel tmp in tmpCollection)
        {
            resultList = resultList.Add(tmp);
        }

        return resultList;
    }
}
