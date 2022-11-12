using Microsoft.AspNetCore.SignalR;

namespace AyBorg.Agent.Hubs;

public class FlowHubContext : Hub
{
    // Just an empyt hub used as context for SignalR and the "real" FlowHub.
    // This is needed because SignalR does not supporting long living hubs.
}