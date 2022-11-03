using Atomy.Web.Shared.Models;

namespace Atomy.Web.Services.Agent;

public interface IAgentOverviewService
{
    IEnumerable<AgentServiceEntry> AgentServices { get; }
    int AgentsCount { get; }
    int ActiveAgentsCount { get; }
    int InactiveAgentsCount { get; }

    Task UpdateAsync();
}