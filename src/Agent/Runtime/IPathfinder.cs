using Atomy.SDK.Common;
using Atomy.SDK.Common.Ports;

namespace Atomy.Agent.Runtime;

internal interface IPathfinder
{
    /// <summary>
    /// Creates a path from the given steps and links.
    /// </summary>
    /// <param name="steps">The steps to create the path from.</param>
    /// <param name="links">The links to create the path from.</param>
    /// <returns>The path.</returns>
    Task<IEnumerable<PathItem>> CreatePathAsync(IEnumerable<IStepProxy> steps, IEnumerable<PortLink> links);
}