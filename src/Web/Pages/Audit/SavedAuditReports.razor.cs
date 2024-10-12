using AyBorg.Authorization;
using AyBorg.Web.Services;
using AyBorg.Web.Shared.Modals;
using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace AyBorg.Web.Pages.Audit;

public partial class SavedAuditReports : ComponentBase
{
    [CascadingParameter] Task<AuthenticationState> AuthenticationState { get; init; } = null!;
    [Inject] IAuditService AuditService { get; init; } = null!;
    [Inject] IDialogService DialogService { get; init; } = null!;
    [Inject] ISnackbar Snackbar { get; init; } = null!;
    private bool _isLoading = false;
    private bool _isFilterHidden = false;
    private readonly List<AuditReport> _auditReports = new();
    private readonly List<AuditReport> _filteredAuditReports = new();
    private readonly List<AuditChangeset> _selectedChangesets = new();
    private DateRange _dateRange = new();
    private bool _isAdminRole = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            _isAdminRole = await IsAdminRoleAsync();
            await LoadReportsAsync();
        }
    }

    private async ValueTask LoadReportsAsync()
    {
        _isLoading = true;
        await InvokeAsync(StateHasChanged);
        _auditReports.Clear();
        _filteredAuditReports.Clear();
        _selectedChangesets.Clear();
        var tmpReports = new List<AuditReport>();
        await foreach (AuditReport report in AuditService.GetReportsAsync())
        {
            tmpReports.Add(report);
        }
        _auditReports.AddRange(tmpReports.OrderByDescending(r => r.Timestamp));
        if(!_auditReports.Any())
        {
            // Nothing to load
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
            return;
        }
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
        _filteredAuditReports.AddRange(_auditReports.Where(r => r.Timestamp.Date >= startDate && r.Timestamp.Date <= endDate));
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

    private async Task DeleteReportClicked(AuditReport report)
    {
        IDialogReference dialog = await DialogService.ShowAsync<ConfirmDialog>("Delete audit report",
                                            new DialogParameters {
                                                { "NeedPassword", true },
                                                { "ContentText", "Are you sure you want to delete the audit report?" }
                                            });
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            if (!await AuditService.TryDeleteReport(report))
            {
                Snackbar.Add("Could not delete audit report!", Severity.Error);
                return;
            }

            await LoadReportsAsync();
        }
    }

    private async ValueTask<bool> IsAdminRoleAsync()
    {
        AuthenticationState authState = await AuthenticationState;
        System.Security.Claims.ClaimsPrincipal user = authState.User;
        if (user == null)
        {
            return false;
        }

        return user.IsInRole(Roles.Administrator);
    }
}
