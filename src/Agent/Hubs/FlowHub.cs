using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Data.Mapper;
using Microsoft.AspNetCore.SignalR;

namespace AyBorg.Agent.Hubs;

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
    /// <param name="remove = false">if set to <c>true</c> [remove].</param>
    public async ValueTask SendLinkChangedAsync(PortLink link, bool remove = false)
    {
        SDK.Data.DTOs.LinkDto dtoLink = _mapper.Map(link);
        await _hubContext.Clients.All.SendAsync("LinkChanged", dtoLink, remove);
    }
}
