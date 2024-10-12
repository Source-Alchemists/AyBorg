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

using Ayborg.Gateway.V1;
using AyBorg.Gateway.Models;
using AyBorg.Types;
using Grpc.Core;

namespace AyBorg.Gateway.Services;

public sealed class RegisterServiceV1 : Register.RegisterBase
{
    private readonly ILogger<RegisterServiceV1> _logger;
    private readonly IKeeperService _keeperService;

    public RegisterServiceV1(ILogger<RegisterServiceV1> logger, IKeeperService keeperService)
    {
        _logger = logger;
        _keeperService = keeperService;
    }

    public override async Task<StatusResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        string callerUrl = request.Url;
        if(request.Url.Contains("{AUTO}", StringComparison.InvariantCultureIgnoreCase))
        {
            // The url in the service configuration is set to 'AUTO' so we get the IP from the caller.
            callerUrl = GetClientAddress(request, context);
        }

        var newServiceEntry = new ServiceEntry
        {
            Name = request.Name,
            UniqueName = request.UniqueName,
            Type = request.Type,
            Url =  callerUrl,
            Version = request.Version
        };

        Guid id = await _keeperService.RegisterAsync(newServiceEntry);
        _logger.LogInformation(new EventId((int)EventLogType.Connect), "Registered {name} ({url}).", newServiceEntry.Name, newServiceEntry.Url);
        return new StatusResponse { Success = true, Id = id.ToString(), ErrorMessage = string.Empty };
    }

    public override async Task<StatusResponse> Unregister(UnregisterRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out Guid id))
            {
                return new StatusResponse { Success = false, Id = request.Id, ErrorMessage = "Invalid id" };
            }

            ServiceEntry? service = _keeperService.Unregister(id);
            _logger.LogInformation(new EventId((int)EventLogType.Disconnect), "Unregistered {serviceName}.", service!.UniqueName);
            return await Task.FromResult(new StatusResponse { Success = true, Id = request.Id, ErrorMessage = string.Empty });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Disconnect), ex, "Failed to unregister");
            return new StatusResponse { Success = false, Id = request.Id, ErrorMessage = "Failed to unregister" };
        }
    }

    public override async Task<StatusResponse> Heartbeat(HeartbeatRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out Guid id))
        {
            return new StatusResponse { Success = false, Id = request.Id, ErrorMessage = "Invalid id" };
        }

        IEnumerable<ServiceEntry> knownServices = await _keeperService.GetAllRegistryEntriesAsync();
        ServiceEntry? serviceEntry = knownServices.FirstOrDefault(x => x.Id == id);
        if (serviceEntry == null)
        {
            return new StatusResponse { Success = false, Id = request.Id, ErrorMessage = "Service not found" };
        }

        await _keeperService.UpdateTimestamp(serviceEntry);
        return new StatusResponse { Success = true, Id = request.Id, ErrorMessage = string.Empty };
    }

    public override async Task<GetServicesResponse> GetServices(GetServicesRequest request, ServerCallContext context)
    {
        IEnumerable<ServiceEntry> knownServices = await _keeperService.GetAllRegistryEntriesAsync();

        if (!string.IsNullOrEmpty(request.Id) && Guid.TryParse(request.Id, out Guid id))
        {
            knownServices = knownServices.Where(s => s.Id.Equals(id));
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            knownServices = knownServices.Where(s => s.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(request.UniqueName))
        {
            knownServices = knownServices.Where(s => s.UniqueName.Equals(request.UniqueName, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(request.Type))
        {
            knownServices = knownServices.Where(s => s.Type.Equals(request.Type, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(request.Version))
        {
            knownServices = knownServices.Where(s => s.Version.Equals(request.Version, StringComparison.OrdinalIgnoreCase));
        }

        return new GetServicesResponse
        {
            Services = { knownServices.Select(s => new ServiceInfo
            {
                Id = s.Id.ToString(),
                Name = s.Name,
                UniqueName = s.UniqueName,
                Type = s.Type,
                Url = s.Url,
                Version = s.Version
            }) }
        };
    }

    private static string GetClientAddress(RegisterRequest request, ServerCallContext context)
    {
        HttpContext httpContext = context.GetHttpContext();
        string clientAddress = context.Peer;
        if (clientAddress.Contains("ipv6", StringComparison.InvariantCultureIgnoreCase))
        {
            clientAddress = clientAddress.Replace("ipv6:", string.Empty);
        }

        clientAddress = clientAddress.Remove(clientAddress.LastIndexOf(':'));
        clientAddress = $"{clientAddress}{request.Url.Substring(request.Url.LastIndexOf(':'))}";

        return httpContext.Request.IsHttps ? $"https://{clientAddress}" : $"http://{clientAddress}";
    }
}
