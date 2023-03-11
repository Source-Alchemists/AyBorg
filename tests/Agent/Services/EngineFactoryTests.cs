using AyBorg.Agent.Runtime;
using AyBorg.SDK.Projects;
using AyBorg.SDK.System.Runtime;
using Microsoft.Extensions.Logging.Abstractions;

namespace AyBorg.Agent.Services.Tests;

public class EngineFactoryTests
{
    private static readonly NullLogger<EngineFactory> s_logger = new();
    private static readonly NullLoggerFactory s_nullLoggerFactory = new();
    private readonly EngineFactory _factory;

    public EngineFactoryTests()
    {
        _factory = new EngineFactory(s_logger, s_nullLoggerFactory);
    }

    [Theory]
    [InlineData(EngineExecutionType.SingleRun)]
    [InlineData(EngineExecutionType.ContinuousRun)]
    public void Test_CreateEngine(EngineExecutionType engineExecutionType)
    {
        // Arrange
        var project = new Project();

        // Act
        using IEngine engine = _factory.CreateEngine(project, engineExecutionType);

        // Assert
        Assert.NotNull(engine);
    }
}
