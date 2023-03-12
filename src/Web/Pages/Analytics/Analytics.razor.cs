using System.Runtime.CompilerServices;
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadEventLogAsync();
        }

        if (!_isLoading)
        {
            _eventLevelOverTimeData.Clear();
            _eventLevelOverTimeLabels.Clear();
            IEnumerable<IGrouping<DateTime, EventLogEntry>> groupedTimeChart = GroupTimeChartToCompactTime(_eventLogTable.FilteredEntries);
            AddTimeSeriesLogLevelIfExists(groupedTimeChart, LogLevel.Trace);
            AddTimeSeriesLogLevelIfExists(groupedTimeChart, LogLevel.Debug);
            AddTimeSeriesLogLevelIfExists(groupedTimeChart, LogLevel.Information);
            AddTimeSeriesLogLevelIfExists(groupedTimeChart, LogLevel.Warning);
            AddTimeSeriesLogLevelIfExists(groupedTimeChart, LogLevel.Error);
            AddTimeSeriesLogLevelIfExists(groupedTimeChart, LogLevel.Critical);
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
                CountAndAddLogLevelToTimeSeries(groupEntry, LogLevel.Trace);
                CountAndAddLogLevelToTimeSeries(groupEntry, LogLevel.Debug);
                CountAndAddLogLevelToTimeSeries(groupEntry, LogLevel.Information);
                CountAndAddLogLevelToTimeSeries(groupEntry, LogLevel.Warning);
                CountAndAddLogLevelToTimeSeries(groupEntry, LogLevel.Error);
                CountAndAddLogLevelToTimeSeries(groupEntry, LogLevel.Critical);
            }

            CreateLogLevelSummary();
            CreateServicesSummary();
            CreateEventsSummary();
            await InvokeAsync(StateHasChanged);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask LoadEventLogAsync()
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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IEnumerable<IGrouping<DateTime, EventLogEntry>> GroupTimeChartToCompactTime(IEnumerable<EventLogEntry> entries)
    {
        if(!entries.Any())
        {
            return entries.OrderBy(e => e.Timestamp).GroupBy(e => new DateTime(e.Timestamp.Year, e.Timestamp.Month, e.Timestamp.Day));
        }
        IEnumerable<IGrouping<DateTime, EventLogEntry>> resultGroup;
        DateTime minTimestamp = entries.Min(e => e.Timestamp);
        DateTime maxTimestamp = entries.Max(e => e.Timestamp);
        TimeSpan diffTime = maxTimestamp - minTimestamp;
        if (diffTime > TimeSpan.FromDays(1))
        {
            resultGroup = entries.OrderBy(e => e.Timestamp).GroupBy(e => new DateTime(e.Timestamp.Year, e.Timestamp.Month, e.Timestamp.Day));
        }
        else if (diffTime > TimeSpan.FromHours(1))
        {
            resultGroup = entries.OrderBy(e => e.Timestamp).GroupBy(e => new DateTime(e.Timestamp.Year, e.Timestamp.Month, e.Timestamp.Day, e.Timestamp.Hour, 0, 0));
        }
        else if (diffTime > TimeSpan.FromMinutes(1))
        {
            resultGroup = entries.OrderBy(e => e.Timestamp).GroupBy(e => new DateTime(e.Timestamp.Year, e.Timestamp.Month, e.Timestamp.Day, e.Timestamp.Hour, e.Timestamp.Minute, 0));
        }
        else
        {
            resultGroup = entries.OrderBy(e => e.Timestamp).GroupBy(e => new DateTime(e.Timestamp.Year, e.Timestamp.Month, e.Timestamp.Day, e.Timestamp.Hour, e.Timestamp.Minute, e.Timestamp.Second));
        }

        return resultGroup;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CreateLogLevelSummary()
    {
        IEnumerable<IGrouping<LogLevel, EventLogEntry>> levelGroup = _eventLogTable.FilteredEntries.GroupBy(e => e.LogLevel).ToList();
        _eventLevelSummaryLabels = levelGroup.Select(g => g.Key.ToString()).ToArray();
        _eventLevelSummaryData = new double[_eventLevelSummaryLabels.Length];
        for (int i = 0; i < _eventLevelSummaryData.Length; i++)
        {
            _eventLevelSummaryData[i] = levelGroup.ElementAt(i).Count();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CreateServicesSummary()
    {
        IEnumerable<IGrouping<string, EventLogEntry>> serviceGroup = _eventLogTable.FilteredEntries.GroupBy(e => e.ServiceUniqueName).ToList();
        _eventServiceSummaryLabels = serviceGroup.Select(g => g.Key).ToArray();
        _eventServiceSummaryData = new double[_eventServiceSummaryLabels.Length];
        for (int i = 0; i < _eventServiceSummaryData.Length; i++)
        {
            _eventServiceSummaryData[i] = serviceGroup.ElementAt(i).Count();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CreateEventsSummary()
    {
        IEnumerable<IGrouping<string, EventLogEntry>> eventIdGroup = _eventLogTable.FilteredEntries.GroupBy(e => e.EventName).ToList();
        _eventIdSummaryLabels = eventIdGroup.Select(g => g.Key).ToArray();
        _eventIdSummaryData = new double[_eventIdSummaryLabels.Length];
        for (int i = 0; i < _eventIdSummaryData.Length; i++)
        {
            _eventIdSummaryData[i] = eventIdGroup.ElementAt(i).Count();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddTimeSeriesLogLevelIfExists(IEnumerable<IGrouping<DateTime, EventLogEntry>> group, LogLevel logLevel)
    {
        if (group.Any(g => g.Any(e => e.LogLevel.Equals(logLevel))))
        {
            _eventLevelOverTimeData.Add(logLevel, new List<double>());
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CountAndAddLogLevelToTimeSeries(IGrouping<DateTime, EventLogEntry> groupEntry, LogLevel logLevel)
    {
        int count = groupEntry.Count(g => g.LogLevel.Equals(logLevel));
        if (_eventLevelOverTimeData.TryGetValue(logLevel, out List<double>? collection))
        {
            collection.Add(count);
        }
    }
}
