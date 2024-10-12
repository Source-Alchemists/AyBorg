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

using AyBorg.Types.Ports;
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
