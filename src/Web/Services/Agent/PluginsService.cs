using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Communication.gRPC;

namespace AyBorg.Web.Services.Agent;

public class PluginsService
{
    private readonly IRpcMapper _rpcMapper;
    private readonly Ayborg.Gateway.Agent.V1.Editor.EditorClient _editorClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginsService"/> class.
    /// </summary>
    /// <param name="rpcMapper">The RPC mapper.</param>
    /// <param name="editorClient">The editor client.</param>
    public PluginsService(IRpcMapper rpcMapper, Ayborg.Gateway.Agent.V1.Editor.EditorClient editorClient)
    {
        _rpcMapper = rpcMapper;
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
            steps.Add(_rpcMapper.FromRpc(s));
        }
        return steps;
    }
}
