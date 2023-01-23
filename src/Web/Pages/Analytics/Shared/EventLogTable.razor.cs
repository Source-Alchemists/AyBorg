using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Analytics.Shared;

public partial class EventLogTable : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public IEnumerable<EventLogEntry> EventEntries { get; set; } = new List<EventLogEntry>();
}
