using AyBorg.Web.Services.Analytics;
using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Analytics;

public partial class Analytics : ComponentBase
{
    [Inject] IEventLogService EventLogService { get; set; } = null!;

    private readonly List<EventLogEntry> _eventRecords = new();
    private string[] _eventLevelSummaryLabels = Array.Empty<string>();
    private double[] _eventLevelSummaryData = Array.Empty<double>();
    private string[] _eventServiceSummaryLabels = Array.Empty<string>();
    private double[] _eventServiceSummaryData = Array.Empty<double>();
    private string[] _eventIdSummaryLabels = Array.Empty<string>();
    private double[] _eventIdSummaryData = Array.Empty<double>();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await foreach (EventLogEntry entry in EventLogService.GetEventsAsync())
            {
                _eventRecords.Insert(0, entry);
                StateHasChanged();
            }

            IEnumerable<IGrouping<LogLevel, EventLogEntry>> levelGroup = _eventRecords.GroupBy(e => e.LogLevel);
            _eventLevelSummaryLabels = levelGroup.Select(g => g.Key.ToString()).ToArray();
            _eventLevelSummaryData = new double[_eventLevelSummaryLabels.Length];
            for (int i = 0; i < _eventLevelSummaryData.Length; i++)
            {
                _eventLevelSummaryData[i] = levelGroup.ElementAt(i).Count();
            }

            IEnumerable<IGrouping<string, EventLogEntry>> serviceGroup = _eventRecords.GroupBy(e => e.ServiceUniqueName);
            _eventServiceSummaryLabels = serviceGroup.Select(g => g.Key).ToArray();
            _eventServiceSummaryData = new double[_eventServiceSummaryLabels.Length];
            for(int i = 0; i < _eventServiceSummaryData.Length; i++)
            {
                _eventServiceSummaryData[i] = serviceGroup.ElementAt(i).Count();
            }

            IEnumerable<IGrouping<string, EventLogEntry>> eventIdGroup = _eventRecords.GroupBy(e => e.EventName);
            _eventIdSummaryLabels = eventIdGroup.Select(g => g.Key).ToArray();
            _eventIdSummaryData = new double[_eventIdSummaryLabels.Length];
            for(int i = 0; i < _eventIdSummaryData.Length; i++)
            {
                _eventIdSummaryData[i] = eventIdGroup.ElementAt(i).Count();
            }

            await InvokeAsync(StateHasChanged);
        }
    }
}
