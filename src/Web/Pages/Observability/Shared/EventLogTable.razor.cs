using System.Text;
using AyBorg.SDK.Common;
using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace AyBorg.Web.Pages.Observability.Shared;

public partial class EventLogTable : ComponentBase
{
    private MudDataGrid<EventLogEntry> _table = null!;

    [Inject] IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] ILogger<EventLogTable> Logger { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public IEnumerable<EventLogEntry> EventEntries { get; set; } = new List<EventLogEntry>();

    public IEnumerable<EventLogEntry> FilteredEntries => _table.FilteredItems;

    public bool IsLoading { get; private set; } = false;

    private async Task DownloadLogAsCsv()
    {
        IsLoading = true;
        try
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Timestamp (UTC);Log Level;Service Name;Service Type;Event;Event ID;Message");
            foreach (EventLogEntry entry in FilteredEntries)
            {
                stringBuilder.AppendLine($"{entry.Timestamp};{entry.LogLevel};{entry.ServiceUniqueName};{entry.ServiceType};{entry.EventName};{entry.EventId};{entry.Message}");
            }

            byte[] file = Encoding.UTF8.GetBytes(stringBuilder.ToString());
            await JSRuntime.InvokeVoidAsync("downloadFile", $"eventLog-{DateTime.UtcNow}.csv", "text/plain", file);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Failed to download event log as CSV.");
        }
        IsLoading = false;
    }

    private static Func<EventLogEntry, string> _timestampCellStyleFunc => x =>
    {
        return "min-width: 200px";
    };
}
