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

public class FlowPortTests
{
    [Fact]
    public void Test_Constructor()
    {
        // Arrange
        var node = new FlowNode(new StepModel());
        var port = new PortModel
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
        var node = new FlowNode(new StepModel());
        var port = new PortModel
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Direction = PortDirection.Input,
            Brand = PortBrand.Boolean,
            Value = "123"
        };

        var flowPort = new FlowPort(node, port);

        PortModel newPort = isEmptyPort ? new PortModel() : port with { Value = "456 " };

        // Act
        flowPort.Update(newPort);

        // Assert
        if (isEmptyPort)
        {
            Assert.Equal(port, flowPort.Port);
        }
        else
        {
            Assert.Equal(newPort, flowPort.Port);
        }
    }
}
