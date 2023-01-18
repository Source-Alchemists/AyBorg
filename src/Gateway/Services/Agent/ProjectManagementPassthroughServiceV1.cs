using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Authorization;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Agent;

public sealed class ProjectManagementPassthroughServiceV1 : ProjectManagement.ProjectManagementBase
{
    private readonly IGrpcChannelService _grpcChannelService;

    public ProjectManagementPassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _grpcChannelService = grpcChannelService;
    }

    public override async Task<GetProjectMetasResponse> GetProjectMetas(GetProjectMetasRequest request, ServerCallContext context)
    {
        ProjectManagement.ProjectManagementClient client = _grpcChannelService.CreateClient<ProjectManagement.ProjectManagementClient>(request.AgentUniqueName);
        return await client.GetProjectMetasAsync(request);
    }

    public override async Task<Empty> ActivateProject(ActivateProjectRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer });
        ProjectManagement.ProjectManagementClient client = _grpcChannelService.CreateClient<ProjectManagement.ProjectManagementClient>(request.AgentUniqueName);
        return await client.ActivateProjectAsync(request, headers);
    }

    public override async Task<Empty> ApproveProject(ApproveProjectRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Reviewer });
        ProjectManagement.ProjectManagementClient client = _grpcChannelService.CreateClient<ProjectManagement.ProjectManagementClient>(request.AgentUniqueName);
        return await client.ApproveProjectAsync(request, headers);
    }

    public override async Task<CreateProjectResponse> CreateProject(CreateProjectRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        ProjectManagement.ProjectManagementClient client = _grpcChannelService.CreateClient<ProjectManagement.ProjectManagementClient>(request.AgentUniqueName);
        return await client.CreateProjectAsync(request, headers);
    }

    public override async Task<Empty> DeleteProject(DeleteProjectRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        ProjectManagement.ProjectManagementClient client = _grpcChannelService.CreateClient<ProjectManagement.ProjectManagementClient>(request.AgentUniqueName);
        return await client.DeleteProjectAsync(request, headers);
    }

    public override async Task<Empty> SaveProject(SaveProjectRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer });
        ProjectManagement.ProjectManagementClient client = _grpcChannelService.CreateClient<ProjectManagement.ProjectManagementClient>(request.AgentUniqueName);
        return await client.SaveProjectAsync(request, headers);
    }
}
