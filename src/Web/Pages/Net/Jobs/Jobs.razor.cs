using System.Collections.Immutable;
using Microsoft.AspNetCore.Components;
using static AyBorg.Web.Services.Net.JobManagerService;

namespace AyBorg.Web.Pages.Net.Jobs;

public partial class Jobs : ComponentBase
{
    private ImmutableList<JobMeta> _jobMetas = ImmutableList<JobMeta>.Empty;
    private bool _isLoading = true;
}
