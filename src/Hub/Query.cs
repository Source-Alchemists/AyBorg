using AyBorg.Hub.Database;
using AyBorg.Hub.DataLoader;
using AyBorg.Hub.Types.Runtime.Agent;
using AyBorg.Hub.Types.Services;

namespace AyBorg.Hub;

public class Query
{
    private readonly IServiceInfoRepository _serviceInfoRepository;

    public Query(IServiceInfoRepository database)
    {
        _serviceInfoRepository = database;
    }

    public RuntimeAgent GetRuntimeAgent(string id)
    {
        return new RuntimeAgent
        {
            Id = "1",
            Name = "Runtime Agent 1"
        };
    }

    public IEnumerable<RuntimeAgent> GetRuntimeAgents()
    {
        return new List<RuntimeAgent>
        {
            new RuntimeAgent
            {
                Id = "1",
                Name = "Runtime Agent 1"
            },
            new RuntimeAgent
            {
                Id = "2",
                Name = "Runtime Agent 2"
            }
        };
    }

    public async Task<ServiceInfo?> GetServiceInfo(string id, ServiceInfoDataLoader dataLoader, CancellationToken cancellationToken)
    {
        return await dataLoader.LoadAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<ServiceInfo>> GetServiceInfos(CancellationToken cancellationToken)
    {
        return await _serviceInfoRepository.GetAsync(cancellationToken);
    }
}
