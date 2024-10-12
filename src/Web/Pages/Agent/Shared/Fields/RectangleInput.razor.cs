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
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class RectangleInput : BaseInput
{
    [Inject] IDialogService DialogService { get; init; } = null!;

    private RectangleModel _value = new();

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _value = (RectangleModel)Value;
    }

    private static string ToLabelValue(RectangleModel rectangle)
    {
        return $"X = {rectangle.X}, Y = {rectangle.Y}, W = {rectangle.Width}, H = {rectangle.Height}";
    }

    private async Task OnEditClicked()
    {
        var dialogParameters = new DialogParameters
        {
            { "Rectangle", _value }
        };
        IDialogReference dialog = await DialogService.ShowAsync<EditRectangleDialog>("Edit rectangle", dialogParameters);
        DialogResult result = await dialog.Result;

        if (!result.Canceled)
        {
            _value = (RectangleModel)result.Data;
            await NotifyValueChangedAsync(_value);
        }
    }
}
