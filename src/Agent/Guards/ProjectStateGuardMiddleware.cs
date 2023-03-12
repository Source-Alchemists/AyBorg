using AyBorg.Agent.Services;
using AyBorg.Data.Agent;
using AyBorg.SDK.Projects;

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
