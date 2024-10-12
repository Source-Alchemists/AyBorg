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

using AyBorg.Communication.gRPC;
using AyBorg.Types.Models;

namespace AyBorg.Web.Services.Agent;

public class PluginsService
{
    private readonly IRpcMapper _rpcMapper;
    private readonly Ayborg.Gateway.Agent.V1.Editor.EditorClient _editorClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginsService"/> class.
    /// </summary>
    /// <param name="rpcMapper">The RPC mapper.</param>
    /// <param name="editorClient">The editor client.</param>
    public PluginsService(IRpcMapper rpcMapper, Ayborg.Gateway.Agent.V1.Editor.EditorClient editorClient)
    {
        _rpcMapper = rpcMapper;
        _editorClient = editorClient;
    }

    /// <summary>
    /// Receive steps from the Agent, using a web service.
    /// </summary>
    public async Task<IEnumerable<StepModel>> ReceiveStepsAsync(string agentUniqueName)
    {
        Ayborg.Gateway.Agent.V1.GetAvailableStepsResponse response = await _editorClient.GetAvailableStepsAsync(new Ayborg.Gateway.Agent.V1.GetAvailableStepsRequest { AgentUniqueName = agentUniqueName });
        var steps = new List<StepModel>();
        foreach (Ayborg.Gateway.Agent.V1.StepDto? s in response.Steps)
        {
            steps.Add(_rpcMapper.FromRpc(s));
        }
        return steps;
    }
}
