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

using System.Runtime.CompilerServices;
using Ayborg.Gateway.Agent.V1;
using AyBorg.Runtime;
using AyBorg.Types;
using AGR = Ayborg.Gateway.Agent.V1.Runtime;
using Grpc.Core;

namespace AyBorg.Web.Services.Agent;

public class RuntimeService : IRuntimeService
{
    private readonly ILogger<RuntimeService> _logger;
    private readonly AGR.RuntimeClient _runtimeClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="stateService">The state service.</param>
    /// <param name="runtimeClient">The runtime client.</param>
    public RuntimeService(ILogger<RuntimeService> logger,
                            AGR.RuntimeClient runtimeClient)
    {
        _logger = logger;
        _runtimeClient = runtimeClient;
    }

    /// <summary>
    /// Gets the status.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns>The status.</returns>
    public async ValueTask<EngineMeta> GetStatusAsync(string serviceUniqueName)
    {
        try
        {
            GetRuntimeStatusResponse response = await _runtimeClient.GetStatusAsync(new GetRuntimeStatusRequest
            {
                AgentUniqueName = serviceUniqueName
            });

            return CreateEngineMeta(response.EngineMetaInfos[0]);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Failed to get status!");
            return null!;
        }
    }

    /// <summary>
    /// Starts the engine.
    /// </summary>
    /// <param name="executionType">Type of the execution.</param>
    /// <returns>The status</returns>
    public async ValueTask<EngineMeta> StartRunAsync(string serviceUniqueName, EngineExecutionType executionType)
    {
        try
        {
            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Start run [{executionType}].", executionType);
            StartRunResponse response = await _runtimeClient.StartRunAsync(new StartRunRequest
            {
                AgentUniqueName = serviceUniqueName,
                EngineExecutionType = (int)executionType,
                EngineId = string.Empty
            });

            return CreateEngineMeta(response.EngineMetaInfos[0]);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Failed to start run");
            return null!;
        }
    }

    /// <summary>
    /// Stops the engine.
    /// </summary>
    /// <returns>The status</returns>
    public async ValueTask<EngineMeta> StopRunAsync(string serviceUniqueName)
    {
        try
        {
            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Stop run.");
            StopRunResponse response = await _runtimeClient.StopRunAsync(new StopRunRequest
            {
                AgentUniqueName = serviceUniqueName,
                EngineId = string.Empty
            });

            return CreateEngineMeta(response.EngineMetaInfos[0]);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Failed to stop run");
            return null!;
        }
    }

    /// <summary>
    /// Aborts the engine.
    /// </summary>
    /// <returns>The status</returns>
    public async ValueTask<EngineMeta> AbortRunAsync(string serviceUniqueName)
    {
        try
        {
            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Abort run.");
            AbortRunResponse response = await _runtimeClient.AbortRunAsync(new AbortRunRequest
            {
                AgentUniqueName = serviceUniqueName,
                EngineId = string.Empty
            });

            return CreateEngineMeta(response.EngineMetaInfos[0]);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Failed to abort run");
            return null!;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static EngineMeta CreateEngineMeta(EngineMetaDto engineMetaInfo) => new()
    {
        Id = Guid.Parse(engineMetaInfo.Id),
        State = (EngineState)engineMetaInfo.State,
        ExecutionType = (EngineExecutionType)engineMetaInfo.ExecutionType,
        StartedAt = engineMetaInfo.StartTime.ToDateTime(),
        StoppedAt = engineMetaInfo.StopTime.ToDateTime()
    };
}
