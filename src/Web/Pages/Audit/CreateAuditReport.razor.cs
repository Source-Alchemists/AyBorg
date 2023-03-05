using System.Runtime.CompilerServices;
using AyBorg.Web.Pages.Audit.Shared;
using AyBorg.Web.Services;
using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Newtonsoft.Json;

namespace AyBorg.Web.Pages.Audit;

public partial class CreateAuditReport : ComponentBase
{
    [Inject] IAuditService AuditService { get; init; } = null!;
    [Inject] ISnackbar Snackbar { get; init; } = null!;

    private readonly Dictionary<ServiceOption, List<AuditChangeset>> _groupedChangesets = new();
    private readonly Dictionary<ServiceOption, AuditChangesetTable> _changesetTables = new();
    private readonly List<CompareGroup> _compareGroups = new();
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

        _isLoading = false;
    }

    private async Task CompareClicked()
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

            _compareGroups.Clear();
            _selectedChangesets = selectedChangesets.OrderBy(c => c.Timestamp).ToList();

            if (!_selectedChangesets.Any())
            {
                _isLoading = false;
                Snackbar.Add("Please select at least one changset!", Severity.Warning);
                return;
            }

            _isFilterHidden = true;

            var tmpCompareGroups = new List<CompareGroup>();
            await foreach (AuditChange change in AuditService.GetAuditChangesAsync(selectedChangesets))
            {
                CompareGroup? compareGroup = tmpCompareGroups.FirstOrDefault(g => g.ChangesetA.Token.Equals(change.ChangesetTokenA) && g.ChangesetB.Token.Equals(change.ChangesetTokenB));
                if (compareGroup == null)
                {
                    AuditChangeset changesetA = _selectedChangesets.FirstOrDefault(c => c.Token.Equals(change.ChangesetTokenA)) ?? new AuditChangeset();
                    AuditChangeset changesetB = _selectedChangesets.First(c => c.Token.Equals(change.ChangesetTokenB));
                    compareGroup = new CompareGroup
                    {
                        ChangesetA = changesetA,
                        ChangesetB = changesetB
                    };
                    tmpCompareGroups.Add(compareGroup);
                }

                compareGroup.Changes.Add(change with
                {
                    ValueA = Prettify(change.ValueA),
                    ValueB = Prettify(change.ValueB)
                });
            }

            foreach (IGrouping<Guid, AuditChangeset> selectedChangesetGroup in selectedChangesets.GroupBy(c => c.ProjectId))
            {
                IOrderedEnumerable<AuditChangeset> g = selectedChangesetGroup.OrderByDescending(g => g.Timestamp);
                if (g.Count() > 1)
                {
                    AuditChangeset lastChangeset = null!;
                    foreach (AuditChangeset changeset in g)
                    {
                        if (lastChangeset == null)
                        {
                            lastChangeset = changeset;
                            continue;
                        }

                        AuditChangeset cB = lastChangeset;
                        AuditChangeset cA = changeset;
                        if (!tmpCompareGroups.Any(t => t.ChangesetA.Equals(cA) && t.ChangesetB.Equals(cB)))
                        {
                            tmpCompareGroups.Add(new CompareGroup
                            {
                                ChangesetA = cA,
                                ChangesetB = cB
                            });
                        }

                        lastChangeset = null!;
                    }
                }
            }

            _compareGroups.AddRange(tmpCompareGroups.OrderByDescending(g => g.ChangesetB.Timestamp));
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void PreviewBackClicked()
    {
        _isFilterHidden = false;
    }

    private static string Prettify(string original)
    {
        try
        {
            using (var stringReader = new StringReader(original))
            using (var stringWriter = new StringWriter())
            {
                var jsonReader = new JsonTextReader(stringReader);
                var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented, IndentChar = '#', Indentation = 9 };
                jsonWriter.WriteToken(jsonReader);
                string intended = stringWriter.ToString();
                intended = intended.Replace("#########", "<br />");
                return intended.Insert(intended.LastIndexOf('}'), "<br />");
            }
        }
        catch
        {
            return original;
        }
    }

    readonly Func<ServiceOption, string> _serviceSelectionConveter = s => s.ServiceUniqueName;

    private sealed record ServiceOption(string ServiceUniqueName, string ServiceType, string Label);
    private sealed record CompareGroup
    {
        public AuditChangeset ChangesetA { get; init; } = null!;
        public AuditChangeset ChangesetB { get; init; } = null!;
        public IList<AuditChange> Changes { get; } = new List<AuditChange>();
    }

}
