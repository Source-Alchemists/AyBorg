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

using Ayborg.Gateway.Agent.V1;
using AyBorg.Communication.gRPC;
using AyBorg.Runtime;
using AyBorg.Runtime.Devices;
using AyBorg.Types.Models;
using Grpc.Core;

namespace AyBorg.Agent.Services.gRPC;

public sealed class DeviceManagerServiceV1 : Ayborg.Gateway.Agent.V1.DeviceManager.DeviceManagerBase
{
    private readonly IDeviceProxyManagerService _deviceManagerService;
    private readonly IRuntimeMapper _runtimeMapper;
    private readonly IRpcMapper _rpcMapper;

    public DeviceManagerServiceV1(IDeviceProxyManagerService deviceManagerService,
                                    IRuntimeMapper runtimeMapper,
                                    IRpcMapper rpcMapper)
    {
        _deviceManagerService = deviceManagerService;
        _runtimeMapper = runtimeMapper;
        _rpcMapper = rpcMapper;
    }

    public override Task<DeviceProviderCollectionResponse> GetAvailableProviders(DefaultAgentRequest request, ServerCallContext context)
    {
        var response = new DeviceProviderCollectionResponse();

        foreach (IDeviceProviderProxy provider in _deviceManagerService.DeviceProviders)
        {
            var provideDto = new DeviceProviderDto
            {
                Name = provider.Name,
                Prefix = NormalizeDevicePrefix(provider.Prefix),
                CanAdd = provider.CanAdd
            };

            foreach (IDeviceProxy device in provider.Devices)
            {
                DeviceDto deviceDto = ToDto(device);

                provideDto.Devices.Add(deviceDto);
            }

            response.DeviceProviders.Add(provideDto);
        }

        return Task.FromResult(response);
    }

    public override async Task<DeviceDto> Add(AddDeviceRequest request, ServerCallContext context)
    {
        string deviceId = request.DeviceId;
        string devicePrefix = request.DevicePrefix;
        if(!string.IsNullOrEmpty(devicePrefix) && !string.IsNullOrWhiteSpace(devicePrefix))
        {
            devicePrefix = NormalizeDevicePrefix(devicePrefix);
            deviceId = $"{devicePrefix}-{deviceId}";

        }
        IDeviceProxy newDevice = await _deviceManagerService.AddAsync(new AddDeviceOptions(request.DeviceProviderName, deviceId));
        return ToDto(newDevice);
    }

    public override async Task<DeviceDto> Remove(RemoveDeviceRequest request, ServerCallContext context)
    {
        IDeviceProxy device = await _deviceManagerService.RemoveAsync(request.DeviceId);
        return ToDto(device);
    }

    public override async Task<DeviceDto> ChangeState(DeviceStateRequest request, ServerCallContext context)
    {
        IDeviceProxy device = await _deviceManagerService.ChangeStateAsync(new ChangeDeviceStateOptions(request.DeviceId, request.Activate));
        return ToDto(device);
    }

    public override Task<DeviceDto> GetDevice(GetDeviceRequest request, ServerCallContext context)
    {
        IDeviceProxy device = _deviceManagerService.DeviceProviders.SelectMany(p => p.Devices).Single(d => d.Id.Equals(request.DeviceId, StringComparison.InvariantCultureIgnoreCase));
        return Task.FromResult(ToDto(device, false));
    }

    public override async Task<DeviceDto> UpdateDevice(UpdateDeviceRequest request, ServerCallContext context)
    {
        var tmpPorts = new List<PortModel>();
        foreach(PortDto? portDto in request.Ports)
        {
            tmpPorts.Add(_rpcMapper.FromRpc(portDto));
        }

        IDeviceProxy device = await _deviceManagerService.UpdateAsync(new UpdateDeviceOptions(request.DeviceId, tmpPorts));
        return ToDto(device, false);
    }

    private DeviceDto ToDto(IDeviceProxy device, bool skipPorts = true)
    {
        var deviceDto = new DeviceDto
        {
            Id = device.Id,
            Name = device.Name,
            Manufacturer = device.Manufacturer,
            IsActive = device.IsActive,
            IsConnected = device.IsConnected
        };

        foreach (string category in device.Categories)
        {
            deviceDto.Categories.Add(category);
        }

        if (!skipPorts)
        {
            foreach (Types.Ports.IPort port in device.Native.Ports)
            {
                deviceDto.Ports.Add(_rpcMapper.ToRpc(_runtimeMapper.FromRuntime(port)));
            }
        }

        return deviceDto;
    }

    private static string NormalizeDevicePrefix(string devicePrefix)
    {
        if(string.IsNullOrEmpty(devicePrefix) || string.IsNullOrWhiteSpace(devicePrefix))
        {
            return devicePrefix;
        }

        string newDevicePrefix = devicePrefix.Trim();
        if(newDevicePrefix.EndsWith('-'))
        {
            newDevicePrefix = newDevicePrefix.Remove(newDevicePrefix.Length-1);
        }

        return newDevicePrefix;
    }
}
