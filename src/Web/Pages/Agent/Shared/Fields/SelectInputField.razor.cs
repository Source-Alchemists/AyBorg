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

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

#nullable disable

public partial class SelectInputField : BaseInputField
{
    private SelectPort.ValueContainer _value;
    private readonly IList<string> _selectedValues = new List<string>();
    private string[] _values = Array.Empty<string>();

    protected override void OnParametersSet()
    {
        _selectedValues.Clear();
        if(Port.Value is SelectPort.ValueContainer value)
        {
            _value = value;
            _selectedValues.Add(_value.SelectedValue ?? string.Empty);
            _values = _value.Values.ToArray();
        }

        base.OnParametersSet();
    }

    private async void OnSelectedValuesChanged(IEnumerable<string> values)
    {
        _value = _value with { SelectedValue = values.FirstOrDefault() };
        await NotifyValueChangedAsync(_value);
    }
}
