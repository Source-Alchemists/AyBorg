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
using AyBorg.Authorization;
using AyBorg.SDK.Authorization;
using Grpc.Core;

namespace AyBorg.Agent.Services.gRPC;

public sealed class StorageServiceV1 : Storage.StorageBase
{
    private readonly IStorageService _storageService;

    public StorageServiceV1(IStorageService storageService)
    {
        _storageService = storageService;
    }

    public override Task<GetDirectoriesResponse> GetDirectories(GetDirectoriesRequest request, ServerCallContext context)
    {
        AuthorizeGuard.ThrowIfNotAuthorized(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer, Roles.Auditor });
        return Task.Factory.StartNew(() =>
        {
            IEnumerable<string> directories = _storageService.GetDirectories(request.Path);
            var result = new GetDirectoriesResponse();
            result.Directories.Add(directories);
            return result;
        });
    }
}
