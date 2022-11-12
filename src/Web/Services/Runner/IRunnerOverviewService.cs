using AyBorg.Web.Shared.Models;

namespace AyBorg.Web.Services.Agent;

public interface IAgentOverviewService
{
    IEnumerable<AgentServiceEntry> AgentServices { get; }
    int AgentsCount { get; }
    int ActiveAgentsCount { get; }
    int InactiveAgentsCount { get; }

    Task UpdateAsync();
}