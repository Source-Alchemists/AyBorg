using AyBorg.Agent.Tests;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Projects;
using AyBorg.SDK.System.Runtime;
using Microsoft.Extensions.Logging.Abstractions;

namespace AyBorg.Agent.Runtime.Tests;

public class EngineTests
{
    private readonly NullLogger<Engine> _logger = new();
    private readonly NullLoggerFactory _loggerFactory = new();

    /// <summary>
    /// Start -> Step1 -> Step2 -> End
    /// </summary>
    [Fact]
    public async ValueTask TestStartSingleRunLinear()
    {
        // Arrange
        var project = new Project();

        var startStep = new StepProxy(MockHelper.CreateStepBodyMock("Start", 0, 1).Object);
        var step1 = new StepProxy(MockHelper.CreateStepBodyMock("Step 1", 1, 1).Object);
        var step2 = new StepProxy(MockHelper.CreateStepBodyMock("Step 2", 1, 1).Object);
        var endStep = new StepProxy(MockHelper.CreateStepBodyMock("End", 1, 0).Object);

        var linkStartStep1 = new PortLink(startStep.Ports.First(p => p.Direction == PortDirection.Output), step1.Ports.First(p => p.Direction == PortDirection.Input));
        var linkStep1Step2 = new PortLink(step1.Ports.First(p => p.Direction == PortDirection.Output), step2.Ports.First(p => p.Direction == PortDirection.Input));
        var linkStep2EndStep = new PortLink(step2.Ports.First(p => p.Direction == PortDirection.Output), endStep.Ports.First(p => p.Direction == PortDirection.Input));

        var steps = new List<IStepProxy> { startStep, step1, step2, endStep };
        var links = new List<PortLink> { linkStartStep1, linkStep1Step2, linkStep2EndStep };

        startStep.Links.Add(linkStartStep1);
        step1.Links.Add(linkStartStep1);
        step1.Links.Add(linkStep1Step2);
        step2.Links.Add(linkStep1Step2);
        step2.Links.Add(linkStep2EndStep);
        endStep.Links.Add(linkStep2EndStep);

        project.Links = links;
        project.Steps = steps;

        using var engine = new Engine(_logger, _loggerFactory, project, EngineExecutionType.SingleRun);
        bool done = false;
        Guid lastIterationId = Guid.Empty;
        bool successful = false;
        engine.IterationFinished += (s, e) => { done = true; lastIterationId = e.IterationId; successful = e.Success; };

        // Act
        bool startResult = await engine.TryStartAsync();
        while (!done)
        {
            await Task.Delay(10);
        }

        // Assert
        Assert.True(startResult);
        Assert.True(successful);
        Assert.All(steps, s => Assert.Equal(lastIterationId, s.IterationId));
    }

    /// <summary>
    /// Start -> Step1 -> Step2a -> End
    ///        |--------> Step2b -^
    /// </summary>
    [Fact]
    public async ValueTask TestStartSingleRunParallel()
    {
        // Arrange
        var project = new Project();

        var startStep = new StepProxy(MockHelper.CreateStepBodyMock("Start", 0, 1).Object);
        var step1 = new StepProxy(MockHelper.CreateStepBodyMock("Step 1", 1, 1).Object);
        var step2a = new StepProxy(MockHelper.CreateStepBodyMock("Step 2a", 1, 1).Object);
        var step2b = new StepProxy(MockHelper.CreateStepBodyMock("Step 2b", 1, 1).Object);
        var endStep = new StepProxy(MockHelper.CreateStepBodyMock("End", 1, 0).Object);

        var linkStartStep1 = new PortLink(startStep.Ports.First(p => p.Direction == PortDirection.Output), step1.Ports.First(p => p.Direction == PortDirection.Input));
        var linkStep1Step2a = new PortLink(step1.Ports.First(p => p.Direction == PortDirection.Output), step2a.Ports.First(p => p.Direction == PortDirection.Input));
        var linkStep1Step2b = new PortLink(step1.Ports.First(p => p.Direction == PortDirection.Output), step2b.Ports.First(p => p.Direction == PortDirection.Input));
        var linkStep2aEndStep = new PortLink(step2a.Ports.First(p => p.Direction == PortDirection.Output), endStep.Ports.First(p => p.Direction == PortDirection.Input));
        var linkStep2bEndStep = new PortLink(step2b.Ports.First(p => p.Direction == PortDirection.Output), endStep.Ports.First(p => p.Direction == PortDirection.Input));

        var steps = new List<IStepProxy> { startStep, step1, step2a, step2b, endStep };
        var links = new List<PortLink> { linkStartStep1, linkStep1Step2a, linkStep1Step2b, linkStep2aEndStep, linkStep2bEndStep };

        startStep.Links.Add(linkStartStep1);
        step1.Links.Add(linkStartStep1);
        step1.Links.Add(linkStep1Step2a);
        step1.Links.Add(linkStep1Step2b);
        step2a.Links.Add(linkStep1Step2a);
        step2a.Links.Add(linkStep2aEndStep);
        step2b.Links.Add(linkStep1Step2b);
        step2b.Links.Add(linkStep2bEndStep);
        endStep.Links.Add(linkStep2aEndStep);
        endStep.Links.Add(linkStep2bEndStep);

        project.Links = links;
        project.Steps = steps;

        using var engine = new Engine(_logger, _loggerFactory, project, EngineExecutionType.SingleRun);
        bool done = false;
        Guid lastIterationId = Guid.Empty;
        bool successful = false;
        engine.IterationFinished += (s, e) => { done = true; lastIterationId = e.IterationId; successful = e.Success; };

        // Act
        bool startResult = await engine.TryStartAsync();

        while (!done)
        {
            await Task.Delay(10);
        }

        // Assert
        Assert.True(startResult);
        Assert.True(successful);
        Assert.All(steps, s => Assert.Equal(lastIterationId, s.IterationId));
    }

    [Fact]
    public async ValueTask Test_StopContinuousRun()
    {
        // Arrange
        var project = new Project();

        using var engine = new Engine(_logger, _loggerFactory, project, EngineExecutionType.ContinuousRun);
        bool done = false;
        Guid lastIterationId = Guid.Empty;
        bool successful = false;
        engine.IterationFinished += (s, e) => { done = true; lastIterationId = e.IterationId; successful = e.Success; };

        // Act
        bool startResult = await engine.TryStartAsync();

        while (!done)
        {
            await Task.Delay(10);
        }

        bool result = await engine.TryStopAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async ValueTask Test_AbortContinuousRun()
    {
        // Arrange
        var project = new Project();

        using var engine = new Engine(_logger, _loggerFactory, project, EngineExecutionType.ContinuousRun);
        bool done = false;
        Guid lastIterationId = Guid.Empty;
        bool successful = false;
        engine.IterationFinished += (s, e) => { done = true; lastIterationId = e.IterationId; successful = e.Success; };

        // Act
        bool startResult = await engine.TryStartAsync();

        while (!done)
        {
            await Task.Delay(10);
        }

        bool result = await engine.TryAbortAsync();

        // Assert
        Assert.True(result);
    }
}
