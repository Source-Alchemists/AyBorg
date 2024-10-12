/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using AyBorg.Types.Models;
using AyBorg.Types.Ports;
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
        var step = new StepModel
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            X = 1,
            Y = 2,
            Ports =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test",
                    Direction = PortDirection.Input,
                    Brand = PortBrand.Boolean,
                    Value = "123"
                }
            ]
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
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void Test_Update(bool isNullPort, bool hasDifferentId)
    {
        // Arrange
        var step = new StepModel
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            X = 1,
            Y = 2,
            ExecutionTimeMs = 0,
            Ports =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test",
                    Direction = PortDirection.Input,
                    Brand = PortBrand.Boolean,
                    Value = "123"
                }
            ]
        };

        var flowNode = new FlowNode(step, false);
        bool isCalled = false;
        flowNode.StepChanged += () =>
        {
            isCalled = true;
        };

        var ports = new List<PortModel>();
        if(!isNullPort)
        {
            PortModel firstPort = step.Ports.First();
            ports.Add(firstPort with { Value = "456", Id = hasDifferentId ? Guid.NewGuid() : firstPort.Id });
        }

        StepModel newStep = isNullPort ? step with { ExecutionTimeMs = 1, Ports = [new()]}
                                  : step with { ExecutionTimeMs = 1, Ports = ports };


        // Act
        flowNode.Update(newStep);

        // Assert
        Assert.True(isCalled);
        if (isNullPort)
        {
            Assert.Equal(1, flowNode.Step.ExecutionTimeMs);
            Assert.Equal(step.Ports.First(), ((FlowPort)flowNode.Ports[0]).Port);
        }
        else if(!isNullPort && !hasDifferentId)
        {
            Assert.Equal(1, flowNode.Step.ExecutionTimeMs);
            Assert.Equal("456", ((FlowPort)flowNode.Ports[0]).Port.Value);
        } else {
            Assert.Equal(1, flowNode.Step.ExecutionTimeMs);
            Assert.Equal("123", ((FlowPort)flowNode.Ports[0]).Port.Value);
        }
    }

    [Fact]
    public void Test_Delete()
    {
        // Arrange
        var flowNode = new FlowNode(new StepModel());
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
