using Autodroid.SDK.System.Runtime;
using Autodroid.Web.Shared.Models;

namespace Autodroid.Web.Services.Agent;

public class AgentOverviewService : IAgentOverviewService
{
    private readonly IRegistryService _registryService;
    private readonly IRuntimeService _runtimeService;
    private readonly IProjectManagementService _projectManagementService;
    private readonly List<AgentServiceEntry> _AgentServices = new();
    
    private int _AgentsCount = 0;
    private int _activeAgentsCount = 0;
    private int _inactiveAgentsCount = 0;

    public IEnumerable<AgentServiceEntry> AgentServices => _AgentServices;
    public int AgentsCount => _AgentsCount;
    public int ActiveAgentsCount => _activeAgentsCount;
    public int InactiveAgentsCount => _inactiveAgentsCount;

    public AgentOverviewService(IRegistryService registryService, IRuntimeService runtimeService, IProjectManagementService projectManagementService)
    {
        _registryService = registryService;
        _runtimeService = runtimeService;
        _projectManagementService = projectManagementService;
    }

    public async Task UpdateAsync()
    {
        var tmpAgentList = new List<AgentServiceEntry>();
        foreach (var Agent in await _registryService!.ReceiveAllAvailableServicesAsync("Autodroid.Agent"))
        {
            var baseUrl = Agent.Url;
            var projectMeta = await _projectManagementService!.GetActiveMetaAsync(baseUrl);
            var status = await _runtimeService!.GetStatusAsync(baseUrl);
            tmpAgentList.Add(new AgentServiceEntry(Agent)
            {
                ActiveProjectName = projectMeta?.Name ?? "None",
                Status = status
            });
        }

        // If there are Agents with the same name, we will add a number to the name.
        foreach (var g in tmpAgentList.GroupBy(x => x.Name))
        {
            int count = 1;
            if (g.Count() > 1)
            {
                foreach (var v in g)
                {
                    v.Name = $"{v.Name} - {count++}";
                }
            }
        }

        _AgentServices.Clear();
        _AgentServices.AddRange(tmpAgentList.OrderBy(x => x.Name));
        _AgentsCount = _AgentServices.Count;
        _activeAgentsCount = _AgentServices.Count(x => x.Status != null && x.Status.State == EngineState.Running);
        _inactiveAgentsCount = _AgentServices.Count(x => x.Status == null || x.Status.State != EngineState.Running);
    }
}