using System.Runtime.CompilerServices;
using AyBorg.Web.Services;
using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Audit;

public partial class CreateAuditReport : ComponentBase
{
    [Inject] IAuditService AuditService { get; init; } = null!;

    private bool _isLoading = true;
    private Dictionary<ServiceOption, List<AuditChangeset>> _groupedChangesets = new();
    private ServiceOption[] _selectableServiceOptions = Array.Empty<ServiceOption>();
    private IEnumerable<ServiceOption> _selectedOptions = new HashSet<ServiceOption>();
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

        DateTime minTimestamp = unsortedChangesets.Min(c => c.Timestamp).ToUniversalTime();
        DateTime maxTimestamp = unsortedChangesets.Max(c => c.Timestamp).ToUniversalTime();
        _dateRange = new DateRange(minTimestamp, maxTimestamp);

        foreach (IGrouping<string, AuditChangeset> group in unsortedChangesets.GroupBy(c => c.ServiceUniqueName))
        {
            var changesets = group.ToList();
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

        _isLoading = false;
    }

    Func<ServiceOption, string> _serviceSelectionConveter = s => s.ServiceUniqueName;

    private record ServiceOption(string ServiceUniqueName, string ServiceType, string Label);

}
