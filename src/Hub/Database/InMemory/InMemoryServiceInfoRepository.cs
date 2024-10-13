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

using System.Collections.Immutable;
using AyBorg.Hub.Types.Services;

namespace AyBorg.Hub.Database.InMemory;

public class InMemoryServiceInfoRepository : IServiceInfoRepository
{
    private ImmutableList<ServiceInfo> _serviceInfos = [];

    public async ValueTask<ServiceInfo?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        ServiceInfo? result = _serviceInfos.Find(x => x.Id == key);
        return await ValueTask.FromResult(result);
    }

    public ValueTask<IQueryable<ServiceInfo>> GetAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(_serviceInfos.AsQueryable());
    }

    public async ValueTask<IQueryable<ServiceInfo>> GetAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken = default)
    {
        List<ServiceInfo> result = [];
        foreach (string key in keys)
        {
            ServiceInfo? info = await GetAsync(key, cancellationToken);
            if (info != null)
            {
                result.Add(info);
            }

        }

        return await ValueTask.FromResult(result.AsQueryable());
    }

    public async ValueTask<ServiceInfo> AddAsync(ServiceInfo entity, CancellationToken cancellationToken = default)
    {
        _serviceInfos = _serviceInfos.Add(entity);
        return await ValueTask.FromResult(entity);
    }

    public async ValueTask<ServiceInfo?> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        ServiceInfo? entity = _serviceInfos.Find(x => x.Id == key);
        if (entity != null)
        {
            _serviceInfos = _serviceInfos.Remove(entity);
        }

        return await ValueTask.FromResult(entity);
    }
    public ValueTask<ServiceInfo> UpdateAsync(ServiceInfo entity, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
