using AyBorg.Data.Agent;

namespace AyBorg.Agent.Services;

public interface IAuditProviderService
{
    ValueTask<bool> TryAddAsync(ProjectRecord project);
}
