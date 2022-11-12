using AyBorg.SDK.Common.Ports;

namespace AyBorg.Agent.Hubs;

public interface  IFlowHub
{
     /// <summary>
     /// Sends the link changed.
     /// </summary>
     /// <param name="link">The link.</param>
     /// <param name="remove">if set to <c>true</c> [remove].</param>
     ValueTask SendLinkChangedAsync(PortLink link, bool remove = false);
}