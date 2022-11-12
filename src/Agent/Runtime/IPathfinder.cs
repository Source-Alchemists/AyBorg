using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Agent.Runtime;

internal interface IPathfinder
{
    /// <summary>
    /// Creates a path from the given steps and links.
    /// </summary>
    /// <param name="steps">The steps to create the path from.</param>
    /// <param name="links">The links to create the path from.</param>
    /// <returns>The path.</returns>
    ValueTask<IEnumerable<PathItem>> CreatePathAsync(IEnumerable<IStepProxy> steps, IEnumerable<PortLink> links);
}