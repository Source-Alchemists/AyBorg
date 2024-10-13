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

namespace AyBorg.Hub.Database;

public interface IRepository<T>
{
    ValueTask<T?> GetAsync(string key, CancellationToken cancellationToken = default);
    ValueTask<IQueryable<T>> GetAsync(CancellationToken cancellationToken = default);
    ValueTask<IQueryable<T>> GetAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken = default);
    ValueTask<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    ValueTask<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    ValueTask<T?> DeleteAsync(string key, CancellationToken cancellationToken = default);
}
