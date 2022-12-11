using Ayborg.Gateway.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;

namespace AyBorg.Gateway.Services.Agent;

public sealed class ProjectManagementPassthroughServiceV1 : AgentProjectManagement.AgentProjectManagementBase
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
        AgentProjectManagement.AgentProjectManagementClient client = CreateClient(request.AgentUniqueName);
        return await client.ActivateProjectAsync(request);
    }

    public override async Task<Empty> ApproveProject(ApproveProjectRequest request, ServerCallContext context)
    {
        AgentProjectManagement.AgentProjectManagementClient client = CreateClient(request.AgentUniqueName);
        return await client.ApproveProjectAsync(request);
    }

    public override async Task<CreateProjectResponse> CreateProject(CreateProjectRequest request, ServerCallContext context)
    {
        AgentProjectManagement.AgentProjectManagementClient client = CreateClient(request.AgentUniqueName);
        return await client.CreateProjectAsync(request);
    }

    public override async Task<Empty> DeleteProject(DeleteProjectRequest request, ServerCallContext context)
    {
        AgentProjectManagement.AgentProjectManagementClient client = CreateClient(request.AgentUniqueName);
        return await client.DeleteProjectAsync(request);
    }

    public override async Task<GetProjectMetasResponse> GetProjectMetas(GetProjectMetasRequest request, ServerCallContext context)
    {
        AgentProjectManagement.AgentProjectManagementClient client = CreateClient(request.AgentUniqueName);
        return await client.GetProjectMetasAsync(request);
    }

    public override async Task<Empty> SaveProject(SaveProjectRequest request, ServerCallContext context)
    {
        AgentProjectManagement.AgentProjectManagementClient client = CreateClient(request.AgentUniqueName);
        return await client.SaveProjectAsync(request);
    }

    private AgentProjectManagement.AgentProjectManagementClient CreateClient(string agentUniqueName)
    {
        if (string.IsNullOrEmpty(agentUniqueName))
        {
            _logger.LogWarning("AgentUniqueName is null or empty");
            throw new RpcException(new Status(StatusCode.InvalidArgument, "AgentUniqueName is null or empty"));
        }

        try
        {
            GrpcChannel channel = _grpcChannelService.GetChannel(agentUniqueName);
            return new AgentProjectManagement.AgentProjectManagementClient(channel);
        }
        catch (KeyNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Agent not found"));
        }
    }
}
