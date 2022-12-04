using AyBorg.Gateway.Models;
using AyBorg.SDK.Data.DAL;
using AyBorg.SDK.Data.DTOs;
using AutoMapper;

namespace AyBorg.Gateway.Mapper;

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
            config.CreateMap<RegistryEntryDto, ServiceEntryRecord>().ReverseMap();
            config.CreateMap<ServiceEntry, RegistryEntryDto>().ReverseMap();
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
    /// <param name="RegistryEntryDto">The service registry entry dto.</param>
    public ServiceEntry Map(RegistryEntryDto serviceEntryDto) => _mapper.Map<ServiceEntry>(serviceEntryDto);
}
