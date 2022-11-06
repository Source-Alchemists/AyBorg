using Autodroid.SDK.Data.Mapper;
using Autodroid.SDK.Common.Ports;
using Microsoft.AspNetCore.SignalR;

namespace Autodroid.Agent.Hubs;

public class FlowHub : IFlowHub
{
    private readonly IHubContext<FlowHubContext> _hubContext;
    private readonly IDtoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowHub"/> class.
    /// </summary>
    public FlowHub(IHubContext<FlowHubContext> hubContext, IDtoMapper mapper)
    {
        _hubContext = hubContext;
        _mapper = mapper;
    }

    /// <summary>
    /// Sends the link changed.
    /// </summary>
    /// <param name="link">The link.</param>
    /// <param name="removed">if set to <c>true</c> [remove].</param>
    public async Task SendLinkChangedAsync(PortLink link, bool removed)
    {
        var dtoLink = _mapper.Map(link);
        await _hubContext.Clients.All.SendAsync("LinkChanged", dtoLink, removed);
    }
}