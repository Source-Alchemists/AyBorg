using AyBorg.Agent.Runtime;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Agent.Tests.Runtime;

#nullable disable

public class PathfinderTests
{
    [Theory]
    [InlineData(1, 1, 1, 1)]
    [InlineData(2, 2, 1, 1)]
    [InlineData(3, 3, 1, 1)]
    [InlineData(6, 6, 1, 1)]
    public async Task TestCreateLinearPathWithConnectedSteps(int numberOfStep, int expectedPathItemCount, int numberOfInputPorts, int numberOfOutputPorts)
    {
        // Arrange
        CreateLinearFlow(numberOfStep, numberOfInputPorts, numberOfOutputPorts, 
                        out var steps, out var links, out var startStep, out var endStep);

        var pathfinder = new Pathfinder();

        // Act
        var resultPathItems = await pathfinder.CreatePathAsync(steps, links);

        // Assert
        Assert.Single(pathfinder.StartSteps);
        Assert.Single(pathfinder.EndSteps);
        Assert.Equal(startStep, pathfinder.StartSteps.First());
        Assert.Equal(endStep, pathfinder.EndSteps.First());
        Assert.Equal(expectedPathItemCount, resultPathItems.Count());
        Assert.Single(resultPathItems.Where(x => x.IsStart));
        Assert.Single(resultPathItems.Where(x => x.IsEnd));
    }

    /// <summary>
    /// S1 -> S2a -> S3
    ///   |-> S2b|-^
    /// </summary>
    [Fact]
    public async Task TestCreateForkPath()
    {
        // Arrange
        var step1 = MockHelper.CreateStepProxyMock("Step 1", 1, 1).Object;
        var step2a = MockHelper.CreateStepProxyMock("Step 2a", 1, 1).Object;
        var step2b = MockHelper.CreateStepProxyMock("Step 2b", 1, 1).Object;
        var step3 = MockHelper.CreateStepProxyMock("Step 3", 2, 1).Object;

        var linkS1S2a = new PortLink(step1.Ports.First(p => p.Direction == PortDirection.Output), step2a.Ports.First(p => p.Direction == PortDirection.Input));
        var linkS1S2b = new PortLink(step1.Ports.First(p => p.Direction == PortDirection.Output), step2b.Ports.First(p => p.Direction == PortDirection.Input));
        var linkS2aS3 = new PortLink(step2a.Ports.First(p => p.Direction == PortDirection.Output), step3.Ports.First(p => p.Direction == PortDirection.Input));
        var linkS2bS3 = new PortLink(step2b.Ports.First(p => p.Direction == PortDirection.Output), step3.Ports.Last(p => p.Direction == PortDirection.Input));

        var steps = new List<IStepProxy> { step1, step2a, step2b, step3 };
        var links = new List<PortLink> { linkS1S2a, linkS1S2b, linkS2aS3, linkS2bS3 };
        step1.Links.Add(linkS1S2a);
        step1.Links.Add(linkS1S2b);
        step2a.Links.Add(linkS1S2a);
        step2a.Links.Add(linkS2aS3);
        step2b.Links.Add(linkS1S2b);
        step2b.Links.Add(linkS2bS3);
        step3.Links.Add(linkS2aS3);
        step3.Links.Add(linkS2bS3);

        var pathfinder = new Pathfinder();

        // Act
        var resultPathItems = await pathfinder.CreatePathAsync(steps, links);

        // Assert
        Assert.Single(pathfinder.StartSteps);
        Assert.Single(pathfinder.EndSteps);
        Assert.Single(pathfinder.ForkSteps);
        Assert.Single(pathfinder.MergeSteps);
        Assert.Equal(1, resultPathItems.Count(p => p.IsStart));
        Assert.Equal(1, resultPathItems.Count(p => p.IsFork));
        Assert.Equal(2, resultPathItems.Count(p => p.IsMerge));
        Assert.Equal(1, resultPathItems.Count(p => p.IsEnd));
    }

    /// <summary>
    /// S1 -> S2a -> S3a -> S4
    ///   |-> S2b -> S3b -^
    /// </summary>
    [Fact]
    public async Task TestCreateForkWithMultipleStepsPath()
    {
        // Arrange
        var step1 = MockHelper.CreateStepProxyMock("Step 1", 1, 1).Object;
        var step2a = MockHelper.CreateStepProxyMock("Step 2a", 1, 1).Object;
        var step2b = MockHelper.CreateStepProxyMock("Step 2b", 1, 1).Object;
        var step3a = MockHelper.CreateStepProxyMock("Step 3a", 1, 1).Object;
        var step3b = MockHelper.CreateStepProxyMock("Step 3b", 1, 1).Object;
        var step4 = MockHelper.CreateStepProxyMock("Step 4", 2, 1).Object;

        var linkS1S2a = new PortLink(step1.Ports.First(p => p.Direction == PortDirection.Output), step2a.Ports.First(p => p.Direction == PortDirection.Input));
        var linkS1S2b = new PortLink(step1.Ports.First(p => p.Direction == PortDirection.Output), step2b.Ports.First(p => p.Direction == PortDirection.Input));
        var linkS2aS3a = new PortLink(step2a.Ports.First(p => p.Direction == PortDirection.Output), step3a.Ports.First(p => p.Direction == PortDirection.Input));
        var linkS2bS3b = new PortLink(step2b.Ports.First(p => p.Direction == PortDirection.Output), step3b.Ports.First(p => p.Direction == PortDirection.Input));
        var linkS3aS4 = new PortLink(step3a.Ports.First(p => p.Direction == PortDirection.Output), step4.Ports.First(p => p.Direction == PortDirection.Input));
        var linkS3bS4 = new PortLink(step3b.Ports.First(p => p.Direction == PortDirection.Output), step4.Ports.Last(p => p.Direction == PortDirection.Input));

        var steps = new List<IStepProxy> { step1, step2a, step2b, step3a, step3b, step4 };
        var links = new List<PortLink> { linkS1S2a, linkS1S2b, linkS2aS3a, linkS2bS3b, linkS3aS4, linkS3bS4 };
        step1.Links.Add(linkS1S2a);
        step1.Links.Add(linkS1S2b);
        step2a.Links.Add(linkS1S2a);
        step2a.Links.Add(linkS2aS3a);
        step2b.Links.Add(linkS1S2b);
        step2b.Links.Add(linkS2bS3b);
        step3a.Links.Add(linkS2aS3a);
        step3a.Links.Add(linkS3aS4);
        step3b.Links.Add(linkS2bS3b);
        step3b.Links.Add(linkS3bS4);
        step4.Links.Add(linkS3aS4);
        step4.Links.Add(linkS3bS4);

        var pathfinder = new Pathfinder();

        // Act
        var resultPathItems = await pathfinder.CreatePathAsync(steps, links);

        // Assert
        Assert.Single(pathfinder.StartSteps);
        Assert.Single(pathfinder.EndSteps);
        Assert.Single(pathfinder.ForkSteps);
        Assert.Single(pathfinder.MergeSteps);
        Assert.Equal(1, resultPathItems.Count(p => p.IsStart));
        Assert.Equal(1, resultPathItems.Count(p => p.IsFork));
        Assert.Equal(2, resultPathItems.Count(p => p.IsMerge));
        Assert.Equal(1, resultPathItems.Count(p => p.IsEnd));
        Assert.Equal(2, resultPathItems.Count(p => !p.IsStart && !p.IsEnd && !p.IsFork && !p.IsMerge));
    }

    /// <summary>
    /// S1 -> S2a -> S3a -> S4
    ///   |->  -> -> S3b -^
    /// </summary>
    [Fact]
    public async Task TestCreateForkWithNonLinearStepsPath()
    {
        // Arrange
        var step1 = MockHelper.CreateStepProxyMock("Step 1", 0, 1).Object;
        var step2a = MockHelper.CreateStepProxyMock("Step 2a", 1, 1).Object;
        var step3a = MockHelper.CreateStepProxyMock("Step 3a", 1, 1).Object;
        var step3b = MockHelper.CreateStepProxyMock("Step 3b", 1, 1).Object;
        var step4 = MockHelper.CreateStepProxyMock("Step 4", 2, 1).Object;

        var linkS1S2a = new PortLink(step1.Ports.First(p => p.Direction == PortDirection.Output), step2a.Ports.First(p => p.Direction == PortDirection.Input));
        var linkS1S3b = new PortLink(step1.Ports.First(p => p.Direction == PortDirection.Output), step3b.Ports.First(p => p.Direction == PortDirection.Input));
        var linkS2aS3a = new PortLink(step2a.Ports.First(p => p.Direction == PortDirection.Output), step3a.Ports.First(p => p.Direction == PortDirection.Input));
        var linkS3aS4 = new PortLink(step3a.Ports.First(p => p.Direction == PortDirection.Output), step4.Ports.First(p => p.Direction == PortDirection.Input));
        var linkS3bS4 = new PortLink(step3b.Ports.First(p => p.Direction == PortDirection.Output), step4.Ports.Last(p => p.Direction == PortDirection.Input));

        var steps = new List<IStepProxy> { step1, step2a, step3a, step3b, step4 };
        var links = new List<PortLink> { linkS1S2a, linkS1S3b, linkS2aS3a, linkS3aS4, linkS3bS4 };

        step1.Links.Add(linkS1S2a);
        step1.Links.Add(linkS1S3b);
        step2a.Links.Add(linkS1S2a);
        step2a.Links.Add(linkS2aS3a);
        step3a.Links.Add(linkS2aS3a);
        step3a.Links.Add(linkS3aS4);
        step3b.Links.Add(linkS1S3b);
        step3b.Links.Add(linkS3bS4);
        step4.Links.Add(linkS3aS4);
        step4.Links.Add(linkS3bS4);

        var pathfinder = new Pathfinder();

        // Act
        var resultPathItems = await pathfinder.CreatePathAsync(steps, links);

        // Assert
        Assert.Single(pathfinder.StartSteps);
        Assert.Single(pathfinder.EndSteps);
        Assert.Single(resultPathItems.Where(p => p.Step.Id.Equals(step1.Id) && p.Successors.Any(s => s.Id.Equals(step3b.Id))));
        Assert.Equal(5, resultPathItems.Count());
        Assert.Equal(1, resultPathItems.Count(p => p.IsFork));
        Assert.Equal(2, resultPathItems.Count(p => p.IsMerge));
    }


    private static void CreateLinearFlow(int numberOfSteps, int numberOfInputPorts, int numberOfOutputPorts, 
                                        out IList<IStepProxy> steps, out IList<PortLink> links, out IStepProxy startStep, out IStepProxy endStep)
    {
        steps = new List<IStepProxy>();
        links = new List<PortLink>();

        IStepProxy lastStep = null;
        startStep = null;
        endStep = null;

        for (int index = 0; index < numberOfSteps; index++)
        {
            var stepMock = MockHelper.CreateStepProxyMock($"Step {index}", numberOfInputPorts, numberOfOutputPorts);
            var step = stepMock.Object;
            startStep ??= step;

            endStep = step;

            if (lastStep != null)
            {
                var sp = lastStep.Ports.First(p => p.Direction == PortDirection.Output);
                var tp = step.Ports.First(p => p.Direction == PortDirection.Input);
                var link = new PortLink(sp, tp);
                lastStep.Links.Add(link);
                step.Links.Add(link);
                links.Add(link);
            }

            steps.Add(step);
            lastStep = step;
        }
    }
}