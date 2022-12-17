using AyBorg.SDK.Communication.gRPC;
using AyBorg.SDK.Common.Models;

namespace AyBorg.Web.Services.Agent;

public class PluginsService
{
    private readonly ILogger<PluginsService> _logger;
    private readonly IAuthorizationHeaderUtilService _authorizationHeaderUtilService;
    private readonly Ayborg.Gateway.V1.AgentEditor.AgentEditorClient _agentEditorClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginsService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="authorizationHeaderUtilService">The authorization header util service.</param>
    public PluginsService(ILogger<PluginsService> logger,
                            IAuthorizationHeaderUtilService authorizationHeaderUtilService,
                            Ayborg.Gateway.V1.AgentEditor.AgentEditorClient agentEditorClient)
    {
        _logger = logger;
        _authorizationHeaderUtilService = authorizationHeaderUtilService;
        _agentEditorClient = agentEditorClient;
    }

    /// <summary>
    /// Receive steps from the Agent, using a web service.
    /// </summary>
    public async Task<IEnumerable<Step>> ReceiveStepsAsync(string agentUniqueName)
    {
        Ayborg.Gateway.V1.GetAvailableStepsResponse response = await _agentEditorClient.GetAvailableStepsAsync(new Ayborg.Gateway.V1.GetAvailableStepsRequest { AgentUniqueName = agentUniqueName });
        var steps = new List<Step>();
        foreach (Ayborg.Gateway.V1.Step? s in response.Steps)
        {
            steps.Add(RpcMapper.FromRpc(s));
        }
        return steps;
    }
}
