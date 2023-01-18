using AyBorg.SDK.Communication.gRPC.Models;
using AyBorg.SDK.System.Runtime;

namespace AyBorg.Agent.Services;

public interface INotifyService
{
    ValueTask SendEngineStateAsync(EngineMeta engineMeta);
    ValueTask SendIterationFinishedAsync(Guid iterationId);
    ValueTask SendAutomationFlowChangedAsync(AgentAutomationFlowChangeArgs args);
}
