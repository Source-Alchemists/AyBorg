using Atomy.SDK.Data.DAL;
using Atomy.SDK.Data.DTOs;
using Atomy.ServiceRegistry.Models;
using AutoMapper;

namespace Atomy.ServiceRegistry.Mapper;

public class DalMapper : IDalMapper
{
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="DalMapper"/> class.
    /// </summary>
    public DalMapper()
    {
        var config = new MapperConfiguration(config =>
        {
            config.CreateMap<ServiceEntry, ServiceEntryRecord>().ReverseMap();
            config.CreateMap<ServiceRegistryEntryDto, ServiceEntryRecord>().ReverseMap();
            config.CreateMap<ServiceEntry, ServiceRegistryEntryDto>().ReverseMap();
        });

        _mapper = new AutoMapper.Mapper(config);
    }

    /// <summary>
    /// Maps the specified service entry.
    /// </summary>
    /// <param name="serviceEntry">The service entry.</param>
    public ServiceEntryRecord Map(ServiceEntry serviceEntry) => _mapper.Map<ServiceEntryRecord>(serviceEntry);

    /// <summary>
    /// Maps the specified service entry record.
    /// </summary>
    /// <param name="serviceEntryRecord">The service entry record.</param>	
    public ServiceEntry Map(ServiceEntryRecord serviceEntryRecord) => _mapper.Map<ServiceEntry>(serviceEntryRecord);

    /// <summary>
    /// Maps the specified service registry entry dto.
    /// </summary>
    /// <param name="serviceRegistryEntryDto">The service registry entry dto.</param>
    public ServiceEntry Map(ServiceRegistryEntryDto serviceEntryDto) => _mapper.Map<ServiceEntry>(serviceEntryDto);
}