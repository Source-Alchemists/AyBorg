using Atomy.SDK.Common;
using Atomy.SDK.Common.Ports;

namespace Atomy.Agent.Runtime;

public class Pathfinder : IPathfinder
{
    /// <summary>
    /// Gets the start steps of the path.
    /// </summary>
    public IEnumerable<IStepProxy> StartSteps { get; private set; } = new List<IStepProxy>();

    /// <summary>
    /// Gets the end steps of the path.
    /// </summary>
    public IEnumerable<IStepProxy> EndSteps { get; private set; } = new List<IStepProxy>();

    /// <summary>
    /// Gets the merge steps of the path.
    /// </summary>
    public IEnumerable<IStepProxy> MergeSteps { get; private set; } = new List<IStepProxy>();

    /// <summary>
    /// Gets the fork steps of the path.
    /// </summary>
    public IEnumerable<IStepProxy> ForkSteps { get; private set; } = new List<IStepProxy>();

    /// <summary>
    /// Creates a path from the given steps and links.
    /// </summary>
    /// <param name="steps">The steps to create the path from.</param>
    /// <param name="links">The links to create the path from.</param>
    /// <returns>The path.</returns>
    public async Task<IEnumerable<PathItem>> CreatePathAsync(IEnumerable<IStepProxy> steps, IEnumerable<PortLink> links)
    {
        StartSteps = await FindStepsWithoutIncomingLinksAsync(steps, links);
        EndSteps = await FindStepsWithoutOutgoingLinksAsync(steps, links);
        MergeSteps = await FindMergeStepsAsync(steps, links);
        ForkSteps = await FindForkStepsAsync(steps, links);

        var noLinkPathItems = new List<PathItem>();
        foreach (var noLinkSteps in FindStepsWithoutLink(steps, links))
        {
            noLinkPathItems.Add(new PathItem(noLinkSteps));
        }

        var startPathItems = new List<PathItem>();
        foreach (var startStep in StartSteps)
        {
            startPathItems.Add(CreateNextPathItem(startStep, steps, links));
        }

        var endPathItems = new List<PathItem>();
        foreach (var endStep in EndSteps)
        {
            if (StartSteps.Contains(endStep))
            {
                // No need to create a path item for the end step if it is also a start step.
                continue;
            }
            endPathItems.Add(new PathItem(endStep));
        }

        var allPathItems = new List<PathItem>(noLinkPathItems.Concat(startPathItems.Concat(CreatePath(startPathItems, steps, links))).Concat(endPathItems));
        var dups = allPathItems.GroupBy(x => x.Id).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
        foreach (var d in dups)
        {
            while (allPathItems.Count(x => x.Id == d) > 1)
            {
                allPathItems.Remove(allPathItems.First(x => x.Id == d));
            }
        }
        // Update all path items with there individual path type
        foreach (var pathItem in allPathItems)
        {
            if (StartSteps.Any(s => s.Id.Equals(pathItem.Id)))
            {
                pathItem.IsStart = true;
                if (!pathItem.Successors.Any())
                {
                    pathItem.IsEnd = true;
                }
            }
            if (!pathItem.Successors.Any())
            {
                pathItem.IsEnd = true;
            }
            if (pathItem.Successors.Any() && MergeSteps.Any(m => pathItem.Successors.Any(x => x.Id.Equals(m.Id))))
            {
                pathItem.IsMerge = true;
            }
            if (ForkSteps.Any(s => s.Id.Equals(pathItem.Id)))
            {
                pathItem.IsFork = true;
            }
        }

        foreach (var pathItem in allPathItems)
        {
            foreach (var pred in allPathItems.Where(p => p.Successors.Any(x => x.Id.Equals(pathItem.Id))))
            {
                pathItem.Predecessors.Add(pred.Step);
            }

            // Ininitialize the step.
            // Internal it will call onInitilaizeAsync for each stepBody.
            await pathItem.Step.InitializeAsync();
        }
        return await Task.FromResult(allPathItems);
    }

    private static IEnumerable<PathItem> CreatePath(IEnumerable<PathItem> lastPathItems, IEnumerable<IStepProxy> steps, IEnumerable<PortLink> links)
    {
        var pathItems = new List<PathItem>();

        if (!lastPathItems.Any()) return pathItems;

        var lpts = lastPathItems;

        while (true)
        {
            int count = 0;
            var nis = new List<PathItem>();
            foreach (var pathItem in lpts)
            {
                var nextItems = new List<PathItem>();
                foreach (var succ in pathItem.Successors)
                {
                    nextItems.Add(CreateNextPathItem(succ, steps, links));
                }
                pathItems.AddRange(nextItems);
                nis.AddRange(nextItems);
                count += nextItems.Count();
            }
            lpts = nis;
            if (count == 0) break;
        }

        return pathItems;
    }

    private static PathItem CreateNextPathItem(IStepProxy currentStep, IEnumerable<IStepProxy> steps, IEnumerable<PortLink> links)
    {
        var sourcePorts = currentStep.Ports.Where(p => links.Any(l => l.SourceId == p.Id));
        var outgoingLinks = links.Where(l => sourcePorts.Any(p => p.Id == l.SourceId));
        var sameTargetLinks = outgoingLinks.GroupBy(l => l.TargetId);

        var nextItem = new PathItem(currentStep);
        foreach (var stls in sameTargetLinks)
        {
            var tps = stls.Select(x => x.Target);
            var targetStep = steps.First(s => s.Ports.Any(p => tps.Any(tp => tp.Id == p.Id)));
            nextItem.Successors.Add(targetStep);
        }
        return nextItem;
    }

    private static IEnumerable<IStepProxy> FindStepsWithoutLink(IEnumerable<IStepProxy> steps, IEnumerable<PortLink> links)
    {
        foreach (var step in steps)
        {
            var stepPorts = step.Ports;
            var stepLinks = links.Where(l => stepPorts.Any(p => p.Id == l.SourceId || p.Id == l.TargetId));

            if (!stepLinks.Any())
            {
                yield return step;
            }
        }
    }

    private static async Task<IEnumerable<IStepProxy>> FindStepsWithoutOutgoingLinksAsync(IEnumerable<IStepProxy> steps, IEnumerable<PortLink> links)
    {
        // Possible endpoints are steps without outgoing links
        var hashSet = new HashSet<IStepProxy>();
        foreach (var s in steps.Where(s => !s.Ports.Where(p => p.Direction == PortDirection.Output).Any()))
        {
            hashSet.Add(s);
        }

        foreach (var s in steps.Where(s => s.Ports.Where(p => p.Direction == PortDirection.Output).All(p => !links.Any(l => l.SourceId.Equals(p.Id)))))
        {
            hashSet.Add(s);
        }

        return await Task.FromResult(hashSet);
    }

    private static async Task<IEnumerable<IStepProxy>> FindStepsWithoutIncomingLinksAsync(IEnumerable<IStepProxy> steps, IEnumerable<PortLink> links)
    {
        var hashSet = new HashSet<IStepProxy>();
        // Possible startpoints are steps without incoming links
        foreach (var s in steps.Where(s => !s.Ports.Where(p => p.Direction == PortDirection.Input).Any()))
        {
            hashSet.Add(s);
        }
        foreach (var s in steps.Where(s => s.Ports.Where(p => p.Direction == PortDirection.Input).All(p => !links.Any(l => l.TargetId.Equals(p.Id)))))
        {
            hashSet.Add(s);
        }
        // var stepsWithoutIncomingPorts = steps.Where(s => s.Ports.Where(p => p.Direction == PortDirection.Input && !links.Any(l => l.TargetId.Equals(p.Id))).Any());

        return await Task.FromResult(hashSet);
    }

    private static async Task<IEnumerable<IStepProxy>> FindMergeStepsAsync(IEnumerable<IStepProxy> steps, IEnumerable<PortLink> links)
    {
        // Merge steps are steps with multiple incoming links
        var mergeSteps = steps.Where(s => s.Ports.Where(p => links.Where(l => l.TargetId.Equals(p.Id)).Any()).Count() > 1);

        return await Task.FromResult(mergeSteps);
    }

    private static async Task<IEnumerable<IStepProxy>> FindForkStepsAsync(IEnumerable<IStepProxy> steps, IEnumerable<PortLink> links)
    {
        // Split steps are steps with multiple outgoing links
        var splitSteps = steps.Where(s => s.Ports.Where(p => p.Direction == PortDirection.Output && links.Where(l => l.SourceId.Equals(p.Id)).Count() > 1).Any());

        return await Task.FromResult(splitSteps);
    }
}