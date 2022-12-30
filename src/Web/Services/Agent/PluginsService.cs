using AyBorg.SDK.Communication.gRPC;
using AyBorg.SDK.Common.Models;

namespace AyBorg.Web.Services.Agent;

public class PluginsService
{
    private readonly ILogger<PluginsService> _logger;
    private readonly Ayborg.Gateway.Agent.V1.Editor.EditorClient _editorClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginsService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="editorClient">The editor client.</param>
    public PluginsService(ILogger<PluginsService> logger,
                            Ayborg.Gateway.Agent.V1.Editor.EditorClient editorClient)
    {
        _logger = logger;
        _editorClient = editorClient;
    }

    /// <summary>
    /// Receive steps from the Agent, using a web service.
    /// </summary>
    public async Task<IEnumerable<Step>> ReceiveStepsAsync(string agentUniqueName)
    {
        Ayborg.Gateway.Agent.V1.GetAvailableStepsResponse response = await _editorClient.GetAvailableStepsAsync(new Ayborg.Gateway.Agent.V1.GetAvailableStepsRequest { AgentUniqueName = agentUniqueName });
        var steps = new List<Step>();
        foreach (Ayborg.Gateway.Agent.V1.StepDto? s in response.Steps)
        {
            steps.Add(RpcMapper.FromRpc(s));
        }
        return steps;
    }
}
