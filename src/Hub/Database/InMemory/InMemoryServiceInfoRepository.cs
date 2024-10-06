using System.Collections.Immutable;
using AyBorg.Hub.Types.Services;

namespace AyBorg.Hub.Database.InMemory;

public class InMemoryServiceInfoRepository : IServiceInfoRepository
{
    private ImmutableList<ServiceInfo> _serviceInfos = [];

    public async ValueTask<IQueryable<ServiceInfo>> GetAsync(string? id)
    {
        List<ServiceInfo> result = [];
        if (id is not null)
        {
            result = _serviceInfos.Where(x => x.Id == id).ToList();
        }
        else
        {
            result = [.. _serviceInfos];
        }

        return await ValueTask.FromResult(result.AsQueryable());
    }

    public async ValueTask<ServiceInfo> AddAsync(ServiceInfo entity)
    {
        _serviceInfos = _serviceInfos.Add(entity);
        return await ValueTask.FromResult(entity);
    }

    public async ValueTask<ServiceInfo?> DeleteAsync(string id)
    {
        ServiceInfo? entity = _serviceInfos.FirstOrDefault(x => x.Id == id);
        if (entity != null)
        {
            _serviceInfos = _serviceInfos.Remove(entity);
        }

        return await ValueTask.FromResult(entity);
    }
    public ValueTask<ServiceInfo> UpdateAsync(ServiceInfo entity) => throw new NotImplementedException();
}
