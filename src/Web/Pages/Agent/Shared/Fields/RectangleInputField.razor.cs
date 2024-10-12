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

using Microsoft.AspNetCore.Components.Web;
using AyBorg.Types.Models;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class RectangleInputField : BaseInputField
{
    private RectangleModel _value = new();
    private bool _isXEditing = false;
    private bool _isYEditing = false;
    private bool _isWidthEditing = false;
    private bool _isHeightEditing = false;

    protected override void OnParametersSet()
    {
        if (Port.Value == null) return;
        if (Port.Value is RectangleModel value)
        {
            _value = value;
            return;
        }

        Port = Port with { Value = _value };
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
