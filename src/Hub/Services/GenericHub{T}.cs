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

using System.Security.Claims;
using AyBorg.Hub.Database;
using AyBorg.Hub.Types.Services;
using Microsoft.AspNetCore.SignalR;

namespace AyBorg.Hub.Services
{
    public class GenericHub<T> : Hub<T> where T : class
    {
        private readonly string _serviceType;
        protected readonly IServiceInfoRepository ServiceInfoRepository;

        public GenericHub(string serviceType, IServiceInfoRepository serviceInfoRepository)
        {
            _serviceType = serviceType;
            ServiceInfoRepository = serviceInfoRepository;
        }

        public override async Task OnConnectedAsync()
        {
            HttpContext httpContext = Context.GetHttpContext()!;
            string? serviceUniqueName = httpContext.User.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;
            string? serviceDisplayName = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.System)?.Value;
            string? version = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Version)?.Value;
            await ServiceInfoRepository.AddAsync(new ServiceInfo {
                Id = Context.ConnectionId,
                UniqueName = serviceUniqueName!,
                Name = serviceDisplayName!,
                Version = version!,
                Type = _serviceType
            });
            await Groups.AddToGroupAsync(Context.ConnectionId, _serviceType);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await ServiceInfoRepository.DeleteAsync(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
