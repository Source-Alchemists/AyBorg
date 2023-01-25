using AyBorg.Web.Pages.Analytics.Shared;
using AyBorg.Web.Services.Analytics;
using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Analytics;

public partial class Analytics : ComponentBase
{
    [Inject] IEventLogService EventLogService { get; set; } = null!;

    private readonly List<EventLogEntry> _eventRecords = new();
    private readonly Dictionary<object, List<double>> _eventLevelOverTimeData = new();
    private readonly List<string> _eventLevelOverTimeLabels = new();
    private EventLogTable _eventLogTable = null!;
    private string[] _eventLevelSummaryLabels = Array.Empty<string>();
    private double[] _eventLevelSummaryData = Array.Empty<double>();
    private string[] _eventServiceSummaryLabels = Array.Empty<string>();
    private double[] _eventServiceSummaryData = Array.Empty<double>();
    private string[] _eventIdSummaryLabels = Array.Empty<string>();
    private double[] _eventIdSummaryData = Array.Empty<double>();
    private bool _isLoading = false;
    private bool _isEventLogTableLoading => _eventLogTable?.IsLoading ?? false;

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            return Task.Factory.StartNew(async () =>
            {
                _isLoading = true;
                int count = 0;
                await foreach (EventLogEntry entry in EventLogService.GetEventsAsync())
                {
                    _eventRecords.Insert(0, entry);
                    // Smoother loading animation
                    count++;
                    if (count > 10)
                    {
                        await InvokeAsync(StateHasChanged);
                        count = 0;
                    }
                }
                _isLoading = false;
            });
        }

        if (!_isLoading)
        {
            _eventLevelOverTimeData.Clear();
            _eventLevelOverTimeLabels.Clear();
            IEnumerable<IGrouping<DateTime, EventLogEntry>> groupedTimeChart = _eventLogTable.FilteredEntries.OrderBy(e => e.Timestamp).GroupBy(e => new DateTime(e.Timestamp.Year, e.Timestamp.Month, e.Timestamp.Day, e.Timestamp.Hour, e.Timestamp.Minute, e.Timestamp.Second));
            if (groupedTimeChart.Any(g => g.Any(e => e.LogLevel.Equals(LogLevel.Trace))))
            {
                _eventLevelOverTimeData.Add(LogLevel.Trace, new List<double>());
            }
            if (groupedTimeChart.Any(g => g.Any(e => e.LogLevel.Equals(LogLevel.Debug))))
            {
                _eventLevelOverTimeData.Add(LogLevel.Debug, new List<double>());
            }
            if (groupedTimeChart.Any(g => g.Any(e => e.LogLevel.Equals(LogLevel.Information))))
            {
                _eventLevelOverTimeData.Add(LogLevel.Information, new List<double>());
            }
            if (groupedTimeChart.Any(g => g.Any(e => e.LogLevel.Equals(LogLevel.Warning))))
            {
                _eventLevelOverTimeData.Add(LogLevel.Warning, new List<double>());
            }
            if (groupedTimeChart.Any(g => g.Any(e => e.LogLevel.Equals(LogLevel.Error))))
            {
                _eventLevelOverTimeData.Add(LogLevel.Error, new List<double>());
            }
            if (groupedTimeChart.Any(g => g.Any(e => e.LogLevel.Equals(LogLevel.Critical))))
            {
                _eventLevelOverTimeData.Add(LogLevel.Critical, new List<double>());
            }
            int timeChartCount = groupedTimeChart.Count();
            int count = 0;
            foreach (IGrouping<DateTime, EventLogEntry> groupEntry in groupedTimeChart)
            {
                if (count == 0 || count == timeChartCount - 1)
                {
                    _eventLevelOverTimeLabels.Add(groupEntry.Key.ToString());
                }
                else
                {
                    _eventLevelOverTimeLabels.Add(string.Empty);
                }
                count++;
                int countTrace = groupEntry.Count(g => g.LogLevel.Equals(LogLevel.Trace));
                int countDebug = groupEntry.Count(g => g.LogLevel.Equals(LogLevel.Debug));
                int countInfo = groupEntry.Count(g => g.LogLevel.Equals(LogLevel.Information));
                int countWarn = groupEntry.Count(g => g.LogLevel.Equals(LogLevel.Warning));
                int countError = groupEntry.Count(g => g.LogLevel.Equals(LogLevel.Error));
                int countCritical = groupEntry.Count(g => g.LogLevel.Equals(LogLevel.Critical));

                if (_eventLevelOverTimeData.TryGetValue(LogLevel.Debug, out List<double>? debugValue))
                {
                    debugValue.Add(countDebug);
                }
                if (_eventLevelOverTimeData.TryGetValue(LogLevel.Trace, out List<double>? traceValue))
                {
                    traceValue.Add(countTrace);
                }
                if (_eventLevelOverTimeData.TryGetValue(LogLevel.Information, out List<double>? infoValue))
                {
                    infoValue.Add(countInfo);
                }
                if (_eventLevelOverTimeData.TryGetValue(LogLevel.Warning, out List<double>? warnValue))
                {
                    warnValue.Add(countWarn);
                }
                if (_eventLevelOverTimeData.TryGetValue(LogLevel.Error, out List<double>? errorValue))
                {
                    errorValue.Add(countError);
                }
                if (_eventLevelOverTimeData.TryGetValue(LogLevel.Critical, out List<double>? criticalValue))
                {
                    criticalValue.Add(countCritical);
                }
            }

            IEnumerable<IGrouping<LogLevel, EventLogEntry>> levelGroup = _eventLogTable.FilteredEntries.GroupBy(e => e.LogLevel).ToList();
            _eventLevelSummaryLabels = levelGroup.Select(g => g.Key.ToString()).ToArray();
            _eventLevelSummaryData = new double[_eventLevelSummaryLabels.Length];
            for (int i = 0; i < _eventLevelSummaryData.Length; i++)
            {
                _eventLevelSummaryData[i] = levelGroup.ElementAt(i).Count();
            }

            IEnumerable<IGrouping<string, EventLogEntry>> serviceGroup = _eventLogTable.FilteredEntries.GroupBy(e => e.ServiceUniqueName).ToList();
            _eventServiceSummaryLabels = serviceGroup.Select(g => g.Key).ToArray();
            _eventServiceSummaryData = new double[_eventServiceSummaryLabels.Length];
            for (int i = 0; i < _eventServiceSummaryData.Length; i++)
            {
                _eventServiceSummaryData[i] = serviceGroup.ElementAt(i).Count();
            }

            IEnumerable<IGrouping<string, EventLogEntry>> eventIdGroup = _eventLogTable.FilteredEntries.GroupBy(e => e.EventName).ToList();
            _eventIdSummaryLabels = eventIdGroup.Select(g => g.Key).ToArray();
            _eventIdSummaryData = new double[_eventIdSummaryLabels.Length];
            for (int i = 0; i < _eventIdSummaryData.Length; i++)
            {
                _eventIdSummaryData[i] = eventIdGroup.ElementAt(i).Count();
            }
            return InvokeAsync(StateHasChanged);
        }

        return Task.CompletedTask;
    }
}
