using Autodroid.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace Autodroid.Web.Pages.Agent.Overview;


public partial class AgentsTable : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public IEnumerable<AgentServiceEntry> Agents { get; set; } = new List<AgentServiceEntry>();
}