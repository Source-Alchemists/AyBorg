using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Projects;
using Moq;

namespace AyBorg.Data.Mapper.Tests;

public class RuntimeMapperTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Test_Step_FromRuntime(bool skipPorts)
    {
        // Arrange
        var runtimeMapper = new RuntimeMapper();
        var port = new NumericPort("Port", PortDirection.Input, 123d);
        var mockStep = new Mock<IStepProxy>();
        mockStep.Setup(m => m.Ports).Returns(new List<IPort> { port });
        mockStep.Setup(m => m.Id).Returns(Guid.NewGuid());
        mockStep.Setup(m => m.Name).Returns("Test");
        mockStep.Setup(m => m.Categories).Returns(new List<string> { "C1", "C2" });
        mockStep.Setup(m => m.X).Returns(1);
        mockStep.Setup(m => m.Y).Returns(2);
        mockStep.Setup(m => m.ExecutionTimeMs).Returns(12);
        mockStep.Setup(m => m.MetaInfo).Returns(new PluginMetaInfo
        {
            Id = Guid.Empty,
            AssemblyName = "T1",
            AssemblyVersion = "T2",
            TypeName = "T3"
        });

        // Act
        SDK.Common.Models.Step stepModel = runtimeMapper.FromRuntime(mockStep.Object, skipPorts);

        // Assert
        Assert.Equal(mockStep.Object.Id, stepModel.Id);
        Assert.Equal(mockStep.Object.Name, stepModel.Name);
        Assert.Equal(mockStep.Object.Categories, stepModel.Categories);
        Assert.Equal(mockStep.Object.X, stepModel.X);
        Assert.Equal(mockStep.Object.Y, stepModel.Y);
        Assert.Equal(mockStep.Object.ExecutionTimeMs, stepModel.ExecutionTimeMs);
        Assert.Equal(mockStep.Object.MetaInfo.Id, stepModel.MetaInfo.Id);
        Assert.Equal(mockStep.Object.MetaInfo.AssemblyName, stepModel.MetaInfo.AssemblyName);
        Assert.Equal(mockStep.Object.MetaInfo.AssemblyVersion, stepModel.MetaInfo.AssemblyVersion);
        Assert.Equal(mockStep.Object.MetaInfo.TypeName, stepModel.MetaInfo.TypeName);
        if (!skipPorts)
        {
            Assert.Equal(mockStep.Object.Ports.Count(), stepModel.Ports!.Count());
            Assert.Equal(mockStep.Object.Ports.First().Name, stepModel.Ports!.First().Name);
            Assert.Equal(mockStep.Object.Ports.First().Direction, stepModel.Ports!.First().Direction);
        }
        else
        {
            Assert.Empty(stepModel.Ports!);
        }
    }
}
