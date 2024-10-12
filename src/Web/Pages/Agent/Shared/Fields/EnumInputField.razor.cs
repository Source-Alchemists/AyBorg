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

using AyBorg.Types.Models;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class EnumInputField : BaseInputField
{
    private EnumModel _value = new();
    private readonly IList<string> _selectedNames = new List<string>();
    private string[] _names = Array.Empty<string>();

    protected override Task OnParametersSetAsync()
    {
        _selectedNames.Clear();
        if (Port.Value is EnumModel value)
        {
            _value = value;
        }

        _selectedNames.Add(_value.Name ?? string.Empty);
        _names = _value.Names ?? Array.Empty<string>();
        return base.OnParametersSetAsync();
    }

    private async void OnSelectedValuesChanged(IEnumerable<string> values)
    {
        _value = _value with { Name = values.FirstOrDefault() };
        await NotifyValueChangedAsync(_value);
    }
}
