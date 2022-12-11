using Ayborg.Gateway.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;

namespace AyBorg.Gateway.Services.Agent;

public sealed class ProjectSettingsPassthroughServiceV1 : AgentProjectSettings.AgentProjectSettingsBase
{
    private readonly ILogger<ProjectManagementPassthroughServiceV1> _logger;
    private readonly IGrpcChannelService _grpcChannelService;

    public ProjectSettingsPassthroughServiceV1(ILogger<ProjectManagementPassthroughServiceV1> logger, IGrpcChannelService grpcChannelService)
    {
        _logger = logger;
        _grpcChannelService = grpcChannelService;
    }

    public override async Task<GetProjectSettingsResponse> GetProjectSettings(GetProjectSettingsRequest request, ServerCallContext context)
    {
        AgentProjectSettings.AgentProjectSettingsClient client = CreateClient(request.AgentUniqueName);
        return await client.GetProjectSettingsAsync(request);
    }

    public override async Task<Empty> UpdateProjectSettings(UpdateProjectSettingsRequest request, ServerCallContext context)
    {
        AgentProjectSettings.AgentProjectSettingsClient client = CreateClient(request.AgentUniqueName);
        return await client.UpdateProjectSettingsAsync(request);
    }

    private AgentProjectSettings.AgentProjectSettingsClient CreateClient(string agentUniqueName)
    {
        if (string.IsNullOrEmpty(agentUniqueName))
        {
            _logger.LogWarning("AgentUniqueName is null or empty");
            throw new RpcException(new Status(StatusCode.InvalidArgument, "AgentUniqueName is null or empty"));
        }

        try
        {
            GrpcChannel channel = _grpcChannelService.GetChannel(agentUniqueName);
            return new AgentProjectSettings.AgentProjectSettingsClient(channel);
        }
        catch (KeyNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Agent not found"));
        }
    }
}
