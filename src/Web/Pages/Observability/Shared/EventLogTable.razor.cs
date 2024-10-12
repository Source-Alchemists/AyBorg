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

using System.Text;
using AyBorg.Types;
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
