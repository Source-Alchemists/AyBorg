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
using AyBorg.Types;
using AyBorg.Types.Models;
using AyBorg.Web.Shared.Models.Agent;

namespace AyBorg.Web.Services.Agent;

public class DeviceManagerService : IDeviceManagerService
{
    private readonly ILogger<DeviceManagerService> _logger;
    private readonly DeviceManager.DeviceManagerClient _deviceManagerClient;
    private readonly IRpcMapper _rpcMapper;

    public DeviceManagerService(ILogger<DeviceManagerService> logger, DeviceManager.DeviceManagerClient deviceManagerClient, IRpcMapper rpcMapper)
    {
        _logger = logger;
        _deviceManagerClient = deviceManagerClient;
        _rpcMapper = rpcMapper;
    }

    public async ValueTask<IReadOnlyCollection<DeviceProviderMeta>> GetDeviceProvidersAsync(string agentUniqueName)
    {
        DeviceProviderCollectionResponse response = await _deviceManagerClient.GetAvailableProvidersAsync(new DefaultAgentRequest
        {
            AgentUniqueName = agentUniqueName
        });

        var result = new List<DeviceProviderMeta>();
        foreach (DeviceProviderDto? dtoProvider in response.DeviceProviders)
        {
            var devices = new List<DeviceMeta>();
            foreach (DeviceDto? dtoDevice in dtoProvider.Devices)
            {
                DeviceMeta device = ToObject(dtoDevice);
                devices.Add(device);
            }

            result.Add(new DeviceProviderMeta
            {
                Name = dtoProvider.Name,
                Prefix = dtoProvider.Prefix,
                CanAdd = dtoProvider.CanAdd,
                Devices = devices
            });
        }

        return result;
    }

    public async ValueTask<DeviceMeta> GetDeviceAsync(CommonDeviceRequestOptions options)
    {
        DeviceDto response = await _deviceManagerClient.GetDeviceAsync(new GetDeviceRequest
        {
            AgentUniqueName = options.AgentUniqueName,
            DeviceId = options.DeviceId
        });

        return ToObject(response);
    }

    public async ValueTask<DeviceMeta> AddDeviceAsync(AddDeviceRequestOptions options)
    {
        DeviceDto response = await _deviceManagerClient.AddAsync(new AddDeviceRequest
        {
            AgentUniqueName = options.AgentUniqueName,
            DeviceProviderName = options.DeviceProviderName,
            DeviceId = options.DeviceId,
            DevicePrefix = options.DevicePrefix
        });

        _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Device added: {DeviceId}", response.Id);
        return ToObject(response);
    }

    public async ValueTask<DeviceMeta> RemoveDeviceAsync(CommonDeviceRequestOptions options)
    {
        DeviceDto response = await _deviceManagerClient.RemoveAsync(new RemoveDeviceRequest
        {
            AgentUniqueName = options.AgentUniqueName,
            DeviceId = options.DeviceId
        });

        _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Device removed: {DeviceId}", response.Id);
        return ToObject(response);
    }

    public async ValueTask<DeviceMeta> ChangeDeviceStateAsync(ChangeDeviceStateRequestOptions options)
    {
        DeviceDto response = await _deviceManagerClient.ChangeStateAsync(new DeviceStateRequest
        {
            AgentUniqueName = options.AgentUniqueName,
            DeviceId = options.DeviceId,
            Activate = options.Activate
        });

        _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Device state changed: {DeviceId}, active: {IsActive}", response.Id, response.IsActive);
        return ToObject(response);
    }

    public async ValueTask<DeviceMeta> UpdateDeviceAsync(UpdateDeviceRequestOptions options)
    {
        var request = new UpdateDeviceRequest{
            AgentUniqueName = options.AgentUniqueName,
            DeviceId = options.DeviceId
        };

        foreach(PortModel port in options.Ports)
        {
            request.Ports.Add(_rpcMapper.ToRpc(port));
        }

        DeviceDto response = await _deviceManagerClient.UpdateDeviceAsync(request);
        _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Device updated: {DeviceId}", response.Id);
        return ToObject(response);
    }

    private DeviceMeta ToObject(DeviceDto dtoDevice)
    {
        var ports = new List<PortModel>();
        foreach (PortDto? portDto in dtoDevice.Ports)
        {
            ports.Add(_rpcMapper.FromRpc(portDto));
        }

        return new DeviceMeta()
        {
            Id = dtoDevice.Id,
            Name = dtoDevice.Name,
            Manufacturer = dtoDevice.Manufacturer,
            IsActive = dtoDevice.IsActive,
            IsConnected = dtoDevice.IsConnected,
            Categories = dtoDevice.Categories,
            Ports = ports
        };
    }

    public sealed record CommonDeviceRequestOptions(string AgentUniqueName, string DeviceId);
    public sealed record AddDeviceRequestOptions(string AgentUniqueName, string DeviceProviderName, string DevicePrefix, string DeviceId);
    public sealed record ChangeDeviceStateRequestOptions(string AgentUniqueName, string DeviceId, bool Activate);
    public sealed record UpdateDeviceRequestOptions(string AgentUniqueName, string DeviceId, IReadOnlyCollection<PortModel> Ports);
}
