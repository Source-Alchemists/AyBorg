using System.Collections.Immutable;
using AyBorg.SDK.Common;
using AyBorg.Web.Services.Cognitive;
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static AyBorg.Web.Services.Cognitive.JobManagerService;

namespace AyBorg.Web.Pages.Cognitive.Jobs;

public partial class Jobs : ComponentBase, IAsyncDisposable
{
    [Inject] ILogger<Jobs> Logger { get; init; } = null!;
    [Inject] IJobManagerService JobManagerService { get; init; } = null!;
    [Inject] ISnackbar Snackbar { get; init; } = null!;
    private ImmutableList<JobMeta> _jobMetas = ImmutableList<JobMeta>.Empty;
    private bool _isLoading = true;
    private Timer _updateTimer = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            try
            {
                _jobMetas = _jobMetas.AddRange((await JobManagerService.GetMetasAsync()).OrderByDescending(x => x.QueueDate));

                _updateTimer = new Timer(async e =>
                {
                    try
                    {
                        IEnumerable<JobMeta> response = await JobManagerService.GetMetasAsync();
                        _jobMetas = _jobMetas.Clear();
                        _jobMetas = _jobMetas.AddRange(response.OrderByDescending(x => x.QueueDate));
                    }
                    catch (RpcException ex)
                    {
                        Logger.LogWarning((int)EventLogType.UserInteraction, ex, "Failed to reload jobs!");
                        Snackbar.Add("Failed to reload jobs!", Severity.Warning);
                    }

                    await InvokeAsync(StateHasChanged);
                }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            }
            catch (RpcException ex)
            {
                Logger.LogWarning((int)EventLogType.UserInteraction, ex, "Failed to get jobs!");
                Snackbar.Add("Failed to get jobs!", Severity.Warning);
            }

            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private bool IsDurationVisible(JobMeta jobMeta)
    {
        return jobMeta.Status == JobStatus.Running || jobMeta.Status == JobStatus.Finished;
    }

    private string CreateDurationString(JobMeta jobMeta)
    {
        DateTime finishedDate = jobMeta.FinishedDate;
        if (jobMeta.Status == JobStatus.Running)
        {
            finishedDate = DateTime.UtcNow;
        }

        TimeSpan duration = finishedDate - jobMeta.QueueDate;
        if (duration.TotalSeconds < 60)
        {
            return $"{Math.Round(duration.TotalSeconds, 0)}s";
        }
        else if (duration.TotalMinutes < 60)
        {
            return $"{duration.Minutes}m {duration.Seconds}s";
        }
        else if (duration.TotalHours < 24)
        {
            return $"{duration.Hours}h {duration.Minutes}m";
        }
        else
        {
            return $"{Math.Round(duration.TotalDays, 0)}d";
        }
    }

    private string CreateQueueStartString(JobMeta jobMeta)
    {
        DateTime now = DateTime.UtcNow;
        TimeSpan duration = now - jobMeta.QueueDate;
        double time;
        string unit;
        if (duration.TotalSeconds < 59.5)
        {
            time = Math.Round(duration.TotalSeconds, 0);
            unit = time == 1 ? "second" : "seconds";
        }
        else if (duration.TotalMinutes < 59.5)
        {
            time = Math.Round(duration.TotalMinutes, 0);
            unit = time == 1 ? "minute" : "minutes";
        }
        else if (duration.TotalHours < 23.5)
        {
            time = Math.Round(duration.TotalHours, 0);
            unit = time == 1 ? "hour" : "hours";
        }
        else
        {
            time = Math.Round(duration.TotalDays, 0);
            unit = time == 1 ? "day" : "days";
        }

        return $"{time} {unit} ago";
    }

    public ValueTask DisposeAsync()
    {
        _updateTimer?.Dispose();
        return ValueTask.CompletedTask;
    }
}
