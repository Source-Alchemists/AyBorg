using Ayborg.Gateway.Audit.V1;
using AyBorg.Data.Agent;
using AyBorg.SDK.Common;
using AyBorg.SDK.System;
using AyBorg.SDK.System.Configuration;
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
            await _auditClient.AddEntryAsync(new AuditEntry
            {
                Token = tokenId.ToString(),
                User = userName,
                ServiceType = ServiceTypes.Agent,
                ServiceUniqueName = _serviceConfiguration.UniqueName,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                Type = (int)AuditEntryType.Project,
                AgentProject = AuditMapper.Map(project)
            });
            return tokenId;
        }
        catch (RpcException ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Audit), ex, "Failed to add audit entry for project [{projectName}].", project.Meta.Name);
        }

        return Guid.Empty;
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
        }

        return false;
    }
}
