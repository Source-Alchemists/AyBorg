/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Globalization;
using AutoMapper;
using AyBorg.Runtime.Devices;
using AyBorg.Types.Models;
using AyBorg.Types.Ports;

namespace AyBorg.Data.Agent;

public sealed class DeviceToStorageMapper : IDeviceToStorageMapper
{
    private readonly Mapper _mapper;

    public DeviceToStorageMapper()
    {
        var config = new MapperConfiguration(config =>
        {
            config.CreateMap<IDeviceProxy, DeviceRecord>();
            config.CreateMap<PluginMetaInfo, PluginMetaInfoRecord>().ReverseMap();

            // Ports
            config.CreateMap<NumericPort, DevicePortRecord>().ForMember(d => d.Value, opt => opt.MapFrom(s => Convert.ToString(s.Value, CultureInfo.InvariantCulture)));
            config.CreateMap<StringPort, DevicePortRecord>();
            config.CreateMap<FolderPort, DevicePortRecord>();
            config.CreateMap<BooleanPort, DevicePortRecord>();
            config.CreateMap<ImagePort, DevicePortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new ImageToRecordConverter()));
            config.CreateMap<RectanglePort, DevicePortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new RectangleToRecordConverter()));
            config.CreateMap<EnumPort, DevicePortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new EnumToRecordConverter()));
            config.CreateMap<SelectPort, DevicePortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new SelectValueToRecordConverter()));
            // Port collections
            config.CreateMap<StringCollectionPort, DevicePortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new CollectionToRecordConverter<string>()));
            config.CreateMap<NumericCollectionPort, DevicePortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new CollectionToRecordConverter<double>()));
            config.CreateMap<RectangleCollectionPort, DevicePortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new CollectionToRecordConverter<RectangleModel>()));
        });

        _mapper = new Mapper(config);
    }

    public DeviceRecord Map(IDeviceProxy deviceProxy)
    {
        return _mapper.Map<DeviceRecord>(deviceProxy);
    }
}
