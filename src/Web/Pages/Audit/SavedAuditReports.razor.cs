using AyBorg.Web.Services;
using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Audit;

public partial class SavedAuditReports : ComponentBase
{
    [Inject] IAuditService AuditService { get; init; } = null!;
    private bool _isLoading = false;
    private bool _isFilterHidden = false;
    private readonly List<AuditReport> _auditReports = new();
    private readonly List<AuditReport> _filteredAuditReports = new();
    private List<AuditChangeset> _selectedChangesets = new();
    private DateRange _dateRange = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await LoadReportsAsync();
        }
    }

    private async ValueTask LoadReportsAsync()
    {
        _isLoading = true;
        _auditReports.Clear();
        var tmpReports = new List<AuditReport>();
        await foreach (AuditReport report in AuditService.GetReportsAsync())
        {
            tmpReports.Add(report);
        }
        _auditReports.AddRange(tmpReports.OrderByDescending(r => r.Timestamp));
        _filteredAuditReports.AddRange(_auditReports);
        DateTime min = _auditReports.Min(r => r.Timestamp);
        DateTime max = _auditReports.Max(r => r.Timestamp);
        _dateRange = new DateRange(min, max);
        _isLoading = false;
        await InvokeAsync(StateHasChanged);
    }

    private void FilterClicked()
    {
        _isLoading = true;
        DateTime startDate = _dateRange.Start?.Date ?? DateTime.MinValue;
        DateTime endDate = _dateRange.End?.Date ?? DateTime.MaxValue;

        _filteredAuditReports.Clear();
        _filteredAuditReports.AddRange(_auditReports.Where(r => r.Timestamp.Date >= startDate && r .Timestamp.Date <= endDate));
        _isLoading = false;
    }

    private void RowClicked(TableRowClickEventArgs<AuditReport> tableRowClickEventArgs)
    {
        _isFilterHidden = true;
        _isLoading = true;
        AuditReport selectedReport = tableRowClickEventArgs.Item;
        _selectedChangesets.Clear();
        _selectedChangesets.AddRange(selectedReport.Changesets);
    }

    private void CompareViewLoaded()
    {
        _isLoading = false;
        StateHasChanged();
    }

    private void PreviewBackClicked()
    {
        _isFilterHidden = false;
    }
}
