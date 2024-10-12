/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using AyBorg.Communication;
using AyBorg.Runtime;
using AyBorg.Web.Shared.Models;

namespace AyBorg.Web.Services.Agent;

public class AgentsOverviewService : IAgentOverviewService
{
    private readonly IRegistryService _registryService;
    private readonly IRuntimeService _runtimeService;
    private readonly IProjectManagementService _projectManagementService;
    private readonly List<AgentServiceEntry> _agentServices = new();

    public IEnumerable<AgentServiceEntry> AgentServices => _agentServices;
    public int AgentsCount { get; private set; } = 0;
    public int ActiveAgentsCount { get; private set; } = 0;
    public int InactiveAgentsCount { get; private set; } = 0;

    public AgentsOverviewService(IRegistryService registryService, IRuntimeService runtimeService, IProjectManagementService projectManagementService)
    {
        _registryService = registryService;
        _runtimeService = runtimeService;
        _projectManagementService = projectManagementService;
    }

    public async Task UpdateAsync()
    {
        var tmpAgentList = new List<AgentServiceEntry>();
        foreach (ServiceInfoEntry Agent in await _registryService!.ReceiveServicesAsync(ServiceTypes.Agent))
        {
            Shared.Models.Agent.ProjectMeta projectMeta = await _projectManagementService!.GetActiveMetaAsync(Agent.UniqueName);
            EngineMeta status = await _runtimeService!.GetStatusAsync(Agent.UniqueName);
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
