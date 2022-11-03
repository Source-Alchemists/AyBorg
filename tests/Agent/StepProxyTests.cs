using Atomy.Agent.Tests.Dummies;

namespace Atomy.Agent.Tests;

public class StepProxyTests
{
    [Fact]
    public void Constructs()
    {
        // Arrange
        var step = new DummyStep();

        // Act
        var proxy = new StepProxy(step);

        // Assert
        Assert.Equal(nameof(DummyStep), proxy.MetaInfo.TypeName);
        Assert.Equal("Dummy", proxy.Name);
        Assert.Equal("Atomy.Agent.Tests", proxy.MetaInfo.AssemblyName);
        Assert.Equal("1.0.0.0", proxy.MetaInfo.AssemblyVersion);
    }
}