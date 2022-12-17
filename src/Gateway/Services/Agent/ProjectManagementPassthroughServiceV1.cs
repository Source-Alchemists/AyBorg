using Ayborg.Gateway.Agent.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Agent;

public sealed class ProjectManagementPassthroughServiceV1 : ProjectManagement.ProjectManagementBase
{
    private readonly ILogger<ProjectManagementPassthroughServiceV1> _logger;
    private readonly IGrpcChannelService _grpcChannelService;

    public ProjectManagementPassthroughServiceV1(ILogger<ProjectManagementPassthroughServiceV1> logger, IGrpcChannelService grpcChannelService)
    {
        _logger = logger;
        _grpcChannelService = grpcChannelService;
    }

    public override async Task<Empty> ActivateProject(ActivateProjectRequest request, ServerCallContext context)
    {
        ProjectManagement.ProjectManagementClient client = _grpcChannelService.CreateClient<ProjectManagement.ProjectManagementClient>(request.AgentUniqueName);
        return await client.ActivateProjectAsync(request);
    }

    public override async Task<Empty> ApproveProject(ApproveProjectRequest request, ServerCallContext context)
    {
        ProjectManagement.ProjectManagementClient client = _grpcChannelService.CreateClient<ProjectManagement.ProjectManagementClient>(request.AgentUniqueName);
        return await client.ApproveProjectAsync(request);
    }

    public override async Task<CreateProjectResponse> CreateProject(CreateProjectRequest request, ServerCallContext context)
    {
        ProjectManagement.ProjectManagementClient client = _grpcChannelService.CreateClient<ProjectManagement.ProjectManagementClient>(request.AgentUniqueName);
        return await client.CreateProjectAsync(request);
    }

    public override async Task<Empty> DeleteProject(DeleteProjectRequest request, ServerCallContext context)
    {
        ProjectManagement.ProjectManagementClient client = _grpcChannelService.CreateClient<ProjectManagement.ProjectManagementClient>(request.AgentUniqueName);
        return await client.DeleteProjectAsync(request);
    }

    public override async Task<GetProjectMetasResponse> GetProjectMetas(GetProjectMetasRequest request, ServerCallContext context)
    {
        ProjectManagement.ProjectManagementClient client = _grpcChannelService.CreateClient<ProjectManagement.ProjectManagementClient>(request.AgentUniqueName);
        return await client.GetProjectMetasAsync(request);
    }

    public override async Task<Empty> SaveProject(SaveProjectRequest request, ServerCallContext context)
    {
        ProjectManagement.ProjectManagementClient client = _grpcChannelService.CreateClient<ProjectManagement.ProjectManagementClient>(request.AgentUniqueName);
        return await client.SaveProjectAsync(request);
    }
}
