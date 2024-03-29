using AyBorg.Agent.Tests.Dummies;
using Microsoft.Extensions.Logging.Abstractions;

namespace AyBorg.Agent.Tests;

public class StepProxyTests
{
    [Fact]
    public void Constructs()
    {
        // Arrange
        var step = new DummyStep();

        // Act
        using var proxy = new StepProxy(new NullLogger<StepProxy>(), step);

        // Assert
        Assert.Equal(nameof(DummyStep), proxy.MetaInfo.TypeName);
        Assert.Equal("Dummy", proxy.Name);
        Assert.Equal("AyBorg.Agent.Tests", proxy.MetaInfo.AssemblyName);
        Assert.Equal("1.0.0.0", proxy.MetaInfo.AssemblyVersion);
    }
}
