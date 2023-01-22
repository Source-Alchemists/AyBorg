using AyBorg.Data.Analytics;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Analytics.Shared;

public partial class EventLogTable : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public IEnumerable<EventRecord> EventEntries { get; set; } = new List<EventRecord>();
}
