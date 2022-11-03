using Atomy.SDK.Common.Ports;

namespace Atomy.Agent.Hubs;

public interface  IFlowHub
{
     /// <summary>
     /// Sends the link changed.
     /// </summary>
     /// <param name="link">The link.</param>
     /// <param name="remove">if set to <c>true</c> [remove].</param>
     Task SendLinkChangedAsync(PortLink link, bool remove = false);
}