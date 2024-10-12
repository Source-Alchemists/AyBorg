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

using System.Text.Json;

using Ayborg.Gateway.Agent.V1;
using AyBorg.Communication;
using AyBorg.Runtime;
using AyBorg.Communication.gRPC;
using AyBorg.Communication.gRPC.Models;

namespace AyBorg.Agent.Services;

public sealed class NotifyService : INotifyService
{
    private readonly IServiceConfiguration _serviceConfiguration;
    private readonly Notify.NotifyClient _notifyClient;

    public NotifyService(IServiceConfiguration serviceConfiguration, Notify.NotifyClient notifyClient)
    {
        _serviceConfiguration = serviceConfiguration;
        _notifyClient = notifyClient;
    }

    public async ValueTask SendEngineStateAsync(EngineMeta engineMeta)
    {
        await _notifyClient.CreateNotificationFromAgentAsync(new NotifyMessage
        {
            AgentUniqueName = _serviceConfiguration.UniqueName,
            Type = (int)NotifyType.AgentEngineStateChanged,
            Payload = JsonSerializer.Serialize(engineMeta)
        });
    }

    public async ValueTask SendIterationFinishedAsync(Guid iterationId)
    {
        await _notifyClient.CreateNotificationFromAgentAsync(new NotifyMessage
        {
            AgentUniqueName = _serviceConfiguration.UniqueName,
            Type = (int)NotifyType.AgentIterationFinished,
            Payload = iterationId.ToString()
        });
    }

    public async ValueTask SendAutomationFlowChangedAsync(AgentAutomationFlowChangeArgs args)
    {
        await _notifyClient.CreateNotificationFromAgentAsync(new NotifyMessage
        {
            AgentUniqueName = _serviceConfiguration.UniqueName,
            Type = (int)NotifyType.AgentAutomationFlowChanged,
            Payload = JsonSerializer.Serialize(args)
        });
    }
}
