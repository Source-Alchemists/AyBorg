using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.Web.Pages.Agent.Editor.Nodes;

namespace AyBorg.Web.Tests.Pages.Agent.Editor.Nodes;

public class FlowNodeTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Test_Constructor(bool isLocked)
    {
        // Arrange
        var step = new Step
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            X = 1,
            Y = 2,
            Ports = new List<Port>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test",
                    Direction = PortDirection.Input,
                    Brand = PortBrand.Boolean,
                    Value = "123"
                }
            }
        };

        // Act
        var flowNode = new FlowNode(step, isLocked);

        // Assert
        Assert.Equal(step.Id.ToString(), flowNode.Id);
        Assert.Equal(step.Name, flowNode.Title);
        Assert.Equal(step.X, flowNode.Position.X);
        Assert.Equal(step.Y, flowNode.Position.Y);
        Assert.Equal(isLocked, flowNode.Locked);
        Assert.Equal(step.Ports!.Count(), flowNode.Ports.Count);
        Assert.Equal(step.Ports!.First().Id.ToString(), flowNode.Ports[0].Id);
        Assert.Equal(isLocked, flowNode.Ports[0].Locked);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Test_Update(bool isNullPort)
    {
        // Arrange
        var step = new Step
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            X = 1,
            Y = 2,
            ExecutionTimeMs = 0,
            Ports = new List<Port>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test",
                    Direction = PortDirection.Input,
                    Brand = PortBrand.Boolean,
                    Value = "123"
                }
            }
        };

        var flowNode = new FlowNode(step, false);
        bool isCalled = false;
        flowNode.StepChanged += () =>
        {
            isCalled = true;
        };

        Step newStep = isNullPort ? step with { ExecutionTimeMs = 1, Ports = new List<Port> { new Port {} } } : step with { ExecutionTimeMs = 1 };
        if(!isNullPort)
        {
            step.Ports.First().Value = "456";
        }

        // Act
        flowNode.Update(newStep);

        // Assert
        Assert.True(isCalled);
        if (isNullPort)
        {
            Assert.Equal(1, flowNode.Step.ExecutionTimeMs);
            Assert.Equal(step.Ports.First(), ((FlowPort)flowNode.Ports[0]).Port);
        }
        else
        {
            Assert.Equal(1, flowNode.Step.ExecutionTimeMs);
            Assert.Equal("456", ((FlowPort)flowNode.Ports[0]).Port.Value);
        }
    }

    [Fact]
    public void Test_Delete()
    {
        // Arrange
        var flowNode = new FlowNode(new Step());
        bool isCalled = false;
        flowNode.OnDelete += () =>
        {
            isCalled = true;
        };

        // Act
        flowNode.Delete();

        // Assert
        Assert.True(isCalled);
    }
}
