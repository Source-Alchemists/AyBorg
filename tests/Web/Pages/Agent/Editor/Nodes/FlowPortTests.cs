using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.Web.Pages.Agent.Editor.Nodes;

namespace AyBorg.Web.Tests.Pages.Agent.Editor.Nodes;

public class FlowPortTests
{
    [Fact]
    public void Test_Constructor()
    {
        // Arrange
        var node = new FlowNode(new Step());
        var port = new Port
        {
            Name = "Test",
            Direction = PortDirection.Input,
            Brand = PortBrand.Boolean
        };

        // Act
        var flowPort = new FlowPort(node, port);

        // Assert
        Assert.Equal(flowPort.Id, flowPort.Id);
        Assert.Equal(port, flowPort.Port);
        Assert.Equal("Test", flowPort.Name);
        Assert.Equal(PortDirection.Input, flowPort.Direction);
        Assert.Equal(PortBrand.Boolean, flowPort.Brand);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Test_Update(bool isEmptyPort)
    {
        // Arrange
        var node = new FlowNode(new Step());
        var port = new Port
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Direction = PortDirection.Input,
            Brand = PortBrand.Boolean,
            Value = "123"
        };

        var flowPort = new FlowPort(node, port);
        bool isCalled = false;
        flowPort.PortChanged += () =>
        {
            isCalled = true;
        };

        Port newPort = isEmptyPort ? new Port() : port with { Value = "456 " };

        // Act
        flowPort.Update(newPort);

        // Assert
        if (isEmptyPort)
        {
            Assert.Equal(port, flowPort.Port);
            Assert.False(isCalled);
        }
        else
        {
            Assert.Equal(newPort, flowPort.Port);
            Assert.True(isCalled);
        }
    }
}
