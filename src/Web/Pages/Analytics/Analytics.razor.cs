using AyBorg.Data.Analytics;
using AyBorg.Web.Services.Analytics;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Analytics;

public partial class Analytics : ComponentBase
{
    [Inject] IEventLogService EventLogService { get; set; } = null!;
    private readonly List<EventRecord> _eventRecords = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await foreach (EventRecord entry in EventLogService.GetEventsAsync())
            {
                _eventRecords.Insert(0, entry);
                StateHasChanged();
            }
        }
    }
}
