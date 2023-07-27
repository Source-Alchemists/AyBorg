using AyBorg.SDK.Common.Models;
using AyBorg.Web.Services.Agent;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AyBorg.Web.Pages.Agent.Editor;


public partial class StepsSelection : ComponentBase
{
    private readonly Dictionary<string, int> _availableCategories = new();
    private IEnumerable<Step> _availableSteps = new List<Step>();
    private IEnumerable<Step> _filteredSteps = new List<Step>();
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

    private static void OnDragStart(DragEventArgs _, Step step)
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
