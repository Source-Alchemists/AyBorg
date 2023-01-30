using AyBorg.Data.Agent;

namespace AyBorg.Agent.Services;

public interface IAuditProviderService
{
    ValueTask<Guid> AddAsync(ProjectRecord project, string userName);
    ValueTask<bool> TryInvalidateAsync(Guid tokenId);
}
