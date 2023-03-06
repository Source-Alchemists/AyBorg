using System.Runtime.CompilerServices;
using AyBorg.Web.Pages.Audit.Shared;
using AyBorg.Web.Services;
using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Audit;

public partial class CreateAuditReport : ComponentBase
{
    [Inject] IAuditService AuditService { get; init; } = null!;
    [Inject] ISnackbar Snackbar { get; init; } = null!;

    private readonly Dictionary<ServiceOption, List<AuditChangeset>> _groupedChangesets = new();
    private readonly Dictionary<ServiceOption, AuditChangesetTable> _changesetTables = new();
    private Dictionary<ServiceOption, List<AuditChangeset>> _filteredGroupedChangesets = new();
    private bool _isLoading = true;
    private bool _isFilterHidden = false;
    private ServiceOption[] _selectableServiceOptions = Array.Empty<ServiceOption>();
    private IEnumerable<ServiceOption> _selectedOptions = new HashSet<ServiceOption>();
    private List<AuditChangeset> _selectedChangesets = new();
    private DateRange _dateRange = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            await LoadChangesetsAsync();
        }

        await InvokeAsync(StateHasChanged);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask LoadChangesetsAsync()
    {
        _isLoading = true;
        _groupedChangesets.Clear();
        var unsortedChangesets = new List<AuditChangeset>();
        await foreach (AuditChangeset changeset in AuditService.GetAuditChangesetsAsync())
        {
            unsortedChangesets.Add(changeset);
        }

        if (!unsortedChangesets.Any())
        {
            _isLoading = false;
            return;
        }

        DateTime minTimestamp = unsortedChangesets.Min(c => c.Timestamp).ToUniversalTime();
        DateTime maxTimestamp = unsortedChangesets.Max(c => c.Timestamp).ToUniversalTime();
        _dateRange = new DateRange(minTimestamp, maxTimestamp);

        foreach (IGrouping<string, AuditChangeset> group in unsortedChangesets.GroupBy(c => c.ServiceUniqueName))
        {
            var changesets = group.OrderByDescending(c => c.Timestamp).ToList();
            string serviceType = changesets.First().ServiceType;
            _groupedChangesets.Add(new ServiceOption(group.Key, serviceType, $"{group.Key} ({serviceType})"), changesets);
        }

        _selectableServiceOptions = new ServiceOption[_groupedChangesets.Keys.Count];
        int count = 0;
        foreach (ServiceOption key in _groupedChangesets.Keys)
        {
            _selectableServiceOptions[count++] = key;
        }

        _selectedOptions = new HashSet<ServiceOption>(_selectableServiceOptions);
        _filteredGroupedChangesets = new Dictionary<ServiceOption, List<AuditChangeset>>(_groupedChangesets);

        _isLoading = false;
    }

    private void CompareClicked()
    {
        try
        {
            _isLoading = true;

            var selectedChangesets = new HashSet<AuditChangeset>();
            foreach (ServiceOption service in _changesetTables.Keys)
            {
                foreach (AuditChangeset changeset in _changesetTables[service].SelectedChangesets)
                {
                    selectedChangesets.Add(changeset);
                }
            }
            _selectedChangesets = selectedChangesets.OrderBy(c => c.Timestamp).ToList();

            if (!_selectedChangesets.Any())
            {
                _isLoading = false;
                Snackbar.Add("Please select at least one changset!", Severity.Warning);
                return;
            }

            _isFilterHidden = true;
        }
        catch
        {
            _isLoading = false;
        }
    }

    private void CompareViewLoaded()
    {
        _isLoading = false;
    }

    private void PreviewBackClicked()
    {
        _isFilterHidden = false;
    }

    private void FilterClicked()
    {
        _selectedChangesets.Clear();
        foreach (KeyValuePair<ServiceOption, AuditChangesetTable> t in _changesetTables)
        {
            t.Value.SelectedChangesets.Clear();
        }

        DateTime startDate = _dateRange.Start?.Date ?? DateTime.MinValue;
        DateTime endDate = _dateRange.End?.Date ?? DateTime.MaxValue;
        var tmpGroup = new Dictionary<ServiceOption, List<AuditChangeset>>(_groupedChangesets.Where(k => _selectedOptions.Any(o => o.ServiceUniqueName.Equals(k.Key.ServiceUniqueName))));
        _filteredGroupedChangesets = new Dictionary<ServiceOption, List<AuditChangeset>>();
        foreach (ServiceOption serviceOption in tmpGroup.Keys)
        {
            _filteredGroupedChangesets.Add(serviceOption, tmpGroup[serviceOption].Where(c => c.Timestamp.Date >= startDate && c.Timestamp.Date <= endDate).ToList());
        }
    }

    readonly Func<ServiceOption, string> _serviceSelectionConveter = s => s.ServiceUniqueName;

    private sealed record ServiceOption(string ServiceUniqueName, string ServiceType, string Label);
}
