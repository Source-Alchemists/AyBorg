using AyBorg.Web.Services;
using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace AyBorg.Web.Pages.Audit.Shared;

public partial class CompareView : ComponentBase
{
    [Inject] IAuditService AuditService { get; init; } = null!;
    [Parameter, EditorRequired] public IEnumerable<AuditChangeset> SelectedChangesets { get; init; } = null!;
    [Parameter] public Action? Loaded { get; init; }

    private List<CompareGroup> _compareGroups = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            List<CompareGroup> tmpCompareGroups = await GetTempCompareGroupsAsync();
            _compareGroups = new List<CompareGroup>(FillMissingCompareGroups(tmpCompareGroups, SelectedChangesets));
            Loaded?.Invoke();
        }
    }

    private async ValueTask<List<CompareGroup>> GetTempCompareGroupsAsync()
    {
        var tmpCompareGroups = new List<CompareGroup>();
        await foreach (AuditChange change in AuditService.GetAuditChangesAsync(SelectedChangesets))
        {
            CompareGroup? compareGroup = tmpCompareGroups.FirstOrDefault(g => g.ChangesetA.Token.Equals(change.ChangesetTokenA) && g.ChangesetB.Token.Equals(change.ChangesetTokenB));
            if (compareGroup == null)
            {
                AuditChangeset changesetA = SelectedChangesets.FirstOrDefault(c => c.Token.Equals(change.ChangesetTokenA)) ?? new AuditChangeset();
                AuditChangeset changesetB = SelectedChangesets.First(c => c.Token.Equals(change.ChangesetTokenB));
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

        return tmpCompareGroups;
    }

    private static IEnumerable<CompareGroup> FillMissingCompareGroups(IEnumerable<CompareGroup> tmpCompareGroups, IEnumerable<AuditChangeset> selectedChangesets)
    {
        var result = new List<CompareGroup>(tmpCompareGroups);
        foreach (IGrouping<Guid, AuditChangeset> selectedChangesetGroup in selectedChangesets.GroupBy(c => c.ProjectId))
        {
            IOrderedEnumerable<AuditChangeset> g = selectedChangesetGroup.OrderByDescending(g => g.Timestamp);
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
                    result.Add(new CompareGroup
                    {
                        ChangesetA = cA,
                        ChangesetB = cB
                    });
                }

                lastChangeset = changeset;
            }
        }

        return result.OrderByDescending(g => g.ChangesetB.Timestamp);
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

    private sealed record CompareGroup
    {
        public AuditChangeset ChangesetA { get; init; } = null!;
        public AuditChangeset ChangesetB { get; init; } = null!;
        public IList<AuditChange> Changes { get; } = new List<AuditChange>();
    }
}
