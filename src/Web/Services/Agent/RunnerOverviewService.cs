using AyBorg.SDK.System.Runtime;
using AyBorg.Web.Shared.Models;

namespace AyBorg.Web.Services.Agent;

public class AgentOverviewService : IAgentOverviewService
{
    private readonly IRegistryService _registryService;
    private readonly IRuntimeService _runtimeService;
    private readonly IProjectManagementService _projectManagementService;
    private readonly List<AgentServiceEntry> _agentServices = new();

    public IEnumerable<AgentServiceEntry> AgentServices => _agentServices;
    public int AgentsCount { get; private set; } = 0;
    public int ActiveAgentsCount { get; private set; } = 0;
    public int InactiveAgentsCount { get; private set; } = 0;

    public AgentOverviewService(IRegistryService registryService, IRuntimeService runtimeService, IProjectManagementService projectManagementService)
    {
        _registryService = registryService;
        _runtimeService = runtimeService;
        _projectManagementService = projectManagementService;
    }

    public async Task UpdateAsync()
    {
        var tmpAgentList = new List<AgentServiceEntry>();
        foreach (ServiceInfoEntry Agent in await _registryService!.ReceiveServicesAsync("AyBorg.Agent"))
        {
            Shared.Models.Agent.ProjectMeta projectMeta = await _projectManagementService!.GetActiveMetaAsync();
            EngineMeta status = await _runtimeService!.GetStatusAsync();
            tmpAgentList.Add(new AgentServiceEntry(Agent)
            {
                ActiveProjectName = projectMeta?.Name ?? "None",
                Status = status
            });
        }

        // If there are Agents with the same name, we will add a number to the name.
        foreach (IGrouping<string, AgentServiceEntry> g in tmpAgentList.GroupBy(x => x.Name))
        {
            int count = 1;
            if (g.Count() > 1)
            {
                foreach (AgentServiceEntry v in g)
                {
                    v.Name = $"{v.Name} - {count++}";
                }
            }
        }

        _agentServices.Clear();
        _agentServices.AddRange(tmpAgentList.OrderBy(x => x.Name));
        AgentsCount = _agentServices.Count;
        ActiveAgentsCount = _agentServices.Count(x => x.Status != null && x.Status.State == EngineState.Running);
        InactiveAgentsCount = _agentServices.Count(x => x.Status == null || x.Status.State != EngineState.Running);
    }
}
