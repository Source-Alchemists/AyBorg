using Ayborg.Gateway.V1;

namespace AyBorg.Gateway.Services.Agent;

public sealed class ProjectManagementPassthroughServiceV1 : AgentProjectManagement.AgentProjectManagementBase
{
    private readonly ILogger<ProjectManagementPassthroughServiceV1> _logger;

    public ProjectManagementPassthroughServiceV1(ILogger<ProjectManagementPassthroughServiceV1> logger)
    {
        _logger = logger;
    }
}
