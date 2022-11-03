using Atomy.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace Atomy.Web.Pages.Agent.Overview;


public partial class AgentsTable : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public IEnumerable<AgentServiceEntry> Agents { get; set; } = new List<AgentServiceEntry>();
}