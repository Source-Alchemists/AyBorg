using AyBorg.Hub.Database;
using AyBorg.Hub.Types.Services;

namespace AyBorg.Hub;

public class Mutation
{
    private readonly IServiceInfoRepository _serviceInfoRepository;

    public Mutation(IServiceInfoRepository database)
    {
        _serviceInfoRepository = database;
    }

    public async Task<ServiceInfo> AddServiceInfo(ServiceInfo input)
    {
        return await _serviceInfoRepository.AddAsync(input);
    }

    public async Task<ServiceInfo?> DeleteServiceInfo(string id)
    {
        return await _serviceInfoRepository.DeleteAsync(id);
    }
}
