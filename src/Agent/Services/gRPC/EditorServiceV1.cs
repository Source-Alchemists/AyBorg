using Ayborg.Gateway.V1;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.SDK.System.Agent;
using Grpc.Core;

namespace AyBorg.Agent.Services.gRPC;

public sealed class EditorServiceV1 : AgentEditor.AgentEditorBase
{
    private readonly ILogger<EditorServiceV1> _logger;
    private readonly IPluginsService _pluginsService;

    public EditorServiceV1(ILogger<EditorServiceV1> logger, IPluginsService pluginsService)
    {
        _logger = logger;
        _pluginsService = pluginsService;
    }

    public override Task<GetAvailableStepsResponse> GetAvailableSteps(GetAvailableStepsRequest request, ServerCallContext context)
    {
        return Task.Factory.StartNew(() =>
        {
            var result = new GetAvailableStepsResponse();
            foreach (SDK.Common.IStepProxy step in _pluginsService.Steps)
            {
                SDK.Data.Bindings.Step stepBinding = RuntimeMapper.FromRumtime(step);
                Step rpcStep = RpcMapper.ToRpc(stepBinding);
                result.Steps.Add(rpcStep);
            }

            return result;
        });
    }
}
