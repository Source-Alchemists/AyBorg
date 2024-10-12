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
using AyBorg.Web.Services.Agent;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AyBorg.Web.Pages.Agent.Editor;


public partial class StepsSelection : ComponentBase
{
    private readonly Dictionary<string, int> _availableCategories = new();
    private IEnumerable<StepModel> _availableSteps = new List<StepModel>();
    private IEnumerable<StepModel> _filteredSteps = new List<StepModel>();
    private IEnumerable<string> _selectedCategories = new List<string>() { "All" };

    private string _searchValue = string.Empty;

    [Parameter] public string ServiceUniqueName { get; set; } = string.Empty;
    [Inject] PluginsService PluginsService { get; set; } = null!;

    protected override async Task OnParametersSetAsync()
    {
        _availableSteps = await PluginsService.ReceiveStepsAsync(ServiceUniqueName);
        _filteredSteps = _availableSteps.OrderBy(c => c.Name);
        CreateCategories();

        await base.OnParametersSetAsync();
    }

    private void CreateCategories()
    {
        IEnumerable<IEnumerable<string>> availableCategoryGroups = _availableSteps.Select(s => s.Categories).GroupBy(c => c).Select(g => g.Key);
        var categories = new List<string>();
        foreach (IEnumerable<string> g in availableCategoryGroups)
        {
            foreach (string c in g)
            {
                if(!categories.Contains(c))
                {
                    categories.Add(c);
                }
            }
        }

        categories = categories.OrderBy(c => c).ToList();

        _availableCategories.Add("All", _availableSteps.Count());
        foreach(string c in categories)
        {
            if(c != "All")
            {
                int count = _availableSteps.Count(s => s.Categories.Contains(c));
                _availableCategories.Add(c, count);
            }
        }
    }

    private void ApplySearchFilter()
    {
        _filteredSteps = _availableSteps.Where(s => s.Name.Contains(_searchValue, StringComparison.InvariantCultureIgnoreCase)).OrderBy(c => c.Name);
        if(!_selectedCategories.Contains("All"))
        {
            _filteredSteps = _filteredSteps.Where(s => s.Categories.Contains(_selectedCategories.First())).OrderBy(c => c.Name);
        }
    }

    private static void OnDragStart(DragEventArgs _, StepModel step)
    {
        DragDropStateHandler.DraggedStep = step;
    }

    private void OnSearchTextChanged(string newValue)
    {
        _searchValue = newValue;
        ApplySearchFilter();
    }

    private void OnSelectedCategoryChanged(IEnumerable<string> values)
    {
        _selectedCategories = values;
        ApplySearchFilter();
    }
}
