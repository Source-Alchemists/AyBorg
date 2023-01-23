using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Analytics.Shared;

public partial class EventLogTable : ComponentBase
{
    private MudDataGrid<EventLogEntry> _table = null!;

    [Parameter]
    [EditorRequired]
    public IEnumerable<EventLogEntry> EventEntries { get; set; } = new List<EventLogEntry>();

    public IEnumerable<EventLogEntry> FilteredEntries => _table.FilteredItems;
}
