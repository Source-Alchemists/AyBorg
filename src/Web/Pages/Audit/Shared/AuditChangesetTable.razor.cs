using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Audit.Shared;

public partial class AuditChangesetTable : ComponentBase
{
    [Parameter, EditorRequired] public IEnumerable<AuditChangeset>? Changesets { get; init; }
    public HashSet<AuditChangeset> SelectedChangesets { get; private set; } = new();

    private void OnSelectedItemsChanged(HashSet<AuditChangeset> changesets)
    {
        SelectedChangesets = changesets;
    }
}
