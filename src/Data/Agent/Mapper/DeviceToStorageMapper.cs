using System.Globalization;
using AutoMapper;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Projects;

namespace AyBorg.Data.Agent;

public sealed class DeviceToStorageMapper : IDeviceToStorageMapper
{
    private readonly Mapper _mapper;

    public DeviceToStorageMapper()
    {
        var config = new MapperConfiguration(config =>
        {
            config.CreateMap<PluginMetaInfo, PluginMetaInfoRecord>().ReverseMap();

            // Ports
            config.CreateMap<NumericPort, DevicePortRecord>().ForMember(d => d.Value, opt => opt.MapFrom(s => Convert.ToString(s.Value, CultureInfo.InvariantCulture)));
            config.CreateMap<StringPort, DevicePortRecord>();
            config.CreateMap<FolderPort, DevicePortRecord>();
            config.CreateMap<BooleanPort, DevicePortRecord>();
            config.CreateMap<ImagePort, DevicePortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new ImageToRecordConverter()));
            config.CreateMap<RectanglePort, DevicePortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new RectangleToRecordConverter()));
            config.CreateMap<EnumPort, DevicePortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new EnumToRecordConverter()));
            // Port collections
            config.CreateMap<StringCollectionPort, DevicePortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new CollectionToRecordConverter<string>()));
            config.CreateMap<NumericCollectionPort, DevicePortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new CollectionToRecordConverter<double>()));
            config.CreateMap<RectangleCollectionPort, DevicePortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new CollectionToRecordConverter<Rectangle>()));
        });

        _mapper = new Mapper(config);
    }

    public DeviceRecord Map(IDeviceProxy deviceProxy)
    {
        return _mapper.Map<DeviceRecord>(deviceProxy);
    }
}
