using AyBorg.Hub.Database;
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

    public async Task<ServiceInfo?> GetServiceInfo(string id)
    {
        return (await _serviceInfoRepository.GetAsync(id)).FirstOrDefault();
    }

    public async Task<IEnumerable<ServiceInfo>> GetServiceInfos()
    {
        return await _serviceInfoRepository.GetAsync(null);
    }
}
