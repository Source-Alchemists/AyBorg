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

using Ayborg.Gateway.Audit.V1;
using AyBorg.Communication;
using AyBorg.Data.Agent;
using AyBorg.Types;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Agent.Services;

public sealed class AuditProviderService : IAuditProviderService
{
    private readonly ILogger<AuditProviderService> _logger;
    private readonly Audit.AuditClient _auditClient;
    private readonly IServiceConfiguration _serviceConfiguration;

    public AuditProviderService(ILogger<AuditProviderService> logger, Audit.AuditClient auditClient, IServiceConfiguration serviceConfiguration)
    {
        _logger = logger;
        _auditClient = auditClient;
        _serviceConfiguration = serviceConfiguration;
    }

    public async ValueTask<Guid> AddAsync(ProjectRecord project, string userName)
    {
        try
        {
            var tokenId = Guid.NewGuid();
            var auditEntry = new AuditEntry
            {
                Token = tokenId.ToString(),
                User = userName,
                ServiceType = ServiceTypes.Agent,
                ServiceUniqueName = _serviceConfiguration.UniqueName,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                AgentProject = AuditMapper.Map(project)
            };
            await _auditClient.AddEntryAsync(auditEntry);
            return tokenId;
        }
        catch (RpcException ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Audit), ex, "Failed to add audit entry for project [{projectName}].", project.Meta.Name);
            return Guid.Empty;
        }
    }

    public async ValueTask<bool> TryInvalidateAsync(Guid tokenId)
    {
        try
        {
            await _auditClient.InvalidateEntryAsync(new InvalidateAuditEntryRequest
            {
                Token = tokenId.ToString(),
                ServiceType = ServiceTypes.Agent
            });
            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Audit), ex, "Failed to invalidate audit entry for token [{tokenId}].", tokenId);
            return false;
        }
    }
}
