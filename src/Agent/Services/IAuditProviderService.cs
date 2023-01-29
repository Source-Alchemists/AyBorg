using AyBorg.Data.Agent;

namespace AyBorg.Agent.Services;

public interface IAuditProviderService
{
    ValueTask<Guid> AddAsync(ProjectRecord project);
    ValueTask<bool> TryInvalidateAsync(Guid tokenId);
}
