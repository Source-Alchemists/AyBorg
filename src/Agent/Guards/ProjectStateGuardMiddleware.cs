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

using AyBorg.Agent.Services;
using AyBorg.Data.Agent;
using AyBorg.Runtime.Projects;

namespace AyBorg.Agent.Guards;

public sealed class ProjectStateGuardMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IProjectManagementService _projectManagementService;

    public ProjectStateGuardMiddleware(RequestDelegate next, IProjectManagementService projectManagementService)
    {
        _next = next;
        _projectManagementService = projectManagementService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string method = context.Request.Method;
        if (method == "GET")
        {
            await _next(context);
            return;
        }

        if (!context.Request.Path.StartsWithSegments("/flow"))
        {
            await _next(context);
            return;
        }

        if (_projectManagementService.ActiveProjectId == Guid.Empty)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("No active project.");
            return;
        }

        IEnumerable<ProjectMetaRecord> projectMetas = await _projectManagementService.GetAllMetasAsync();
        ProjectMetaRecord? projectMeta = projectMetas.FirstOrDefault(meta => meta.Id == _projectManagementService.ActiveProjectId);
        if (projectMeta == null)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Active project is not available.");
            return;
        }

        if (projectMeta.State != ProjectState.Draft)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Active project is not in draft state.");
            return;
        }

        await _next(context);
    }
}

public static class ProjectStateGuardMiddlewareExtensions
{
    public static IApplicationBuilder UseProjectStateGuardMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ProjectStateGuardMiddleware>();
    }
}
