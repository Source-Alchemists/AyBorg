using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Dashboard;

public partial class WidgetsSelection : ComponentBase
{
    private string _searchValue = string.Empty;

    private void ApplySearchFilter()
    {
    }

    private void OnSearchTextChanged(string newValue)
    {
        _searchValue = newValue;
        ApplySearchFilter();
    }
}
