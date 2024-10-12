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

using AyBorg.Runtime;
using AyBorg.Types.Models;
using AyBorg.Types.Ports;
using ImageTorque;
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
        StepModel stepModel = runtimeMapper.FromRuntime(mockStep.Object, skipPorts);

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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Test_FromRuntime_StepProxy(bool skipPorts)
    {
        // Arrange
        RuntimeMapper service = new();
        var mockStepProxy = new Mock<IStepProxy>();
        mockStepProxy.Setup(m => m.ExecutionTimeMs).Returns(1);
        mockStepProxy.Setup(m => m.Id).Returns(Guid.NewGuid());
        mockStepProxy.Setup(m => m.MetaInfo).Returns(new PluginMetaInfo
        {
            Id = Guid.NewGuid(),
            AssemblyName = "Test_Assembly",
            AssemblyVersion = "123",
            TypeName = "Test_Type"
        });
        mockStepProxy.Setup(m => m.Name).Returns("Test_Step");
        mockStepProxy.Setup(m => m.X).Returns(2);
        mockStepProxy.Setup(m => m.Y).Returns(3);
        mockStepProxy.Setup(m => m.Ports).Returns(new List<IPort> {
            new StringPort("P1", PortDirection.Input, "Test_Value"),
            new NumericPort("P2", PortDirection.Output, 42),
            new NumericPort("P3", PortDirection.Output, 0),
        });

        IStepProxy stepProxy = mockStepProxy.Object;

        // Act
        StepModel result = service.FromRuntime(stepProxy, skipPorts);

        // Assert
        Assert.Equal(stepProxy.Id, result.Id);
        Assert.Equal(stepProxy.Name, result.Name);
        Assert.Equal(stepProxy.X, result.X);
        Assert.Equal(stepProxy.Y, result.Y);
        Assert.Equal(stepProxy.ExecutionTimeMs, result.ExecutionTimeMs);
        Assert.Equal(stepProxy.MetaInfo, result.MetaInfo);

        if (skipPorts)
        {
            Assert.Empty(result.Ports!);
        }
        else
        {
            Assert.Equal(stepProxy.Ports.Count(), result.Ports!.Count());
            Assert.All(result.Ports!, Assert.NotNull);
        }
    }

    [Theory]
    [InlineData(PortBrand.Boolean, true)]
    [InlineData(PortBrand.Boolean, false)]
    [InlineData(PortBrand.Numeric, 42)]
    [InlineData(PortBrand.String, "test")]
    [InlineData(PortBrand.Folder, "/test")]
    [InlineData(PortBrand.Enum, PortDirection.Output)]
    [InlineData(PortBrand.Rectangle, null)]
    [InlineData(PortBrand.Image, null)]
    public void Test_FromRuntime_Port(PortBrand portBrand, object? value)
    {
        // Arrange
        RuntimeMapper service = new();
        if (portBrand == PortBrand.Rectangle)
        {
            value = new ImageTorque.Rectangle(1, 2, 3, 4);
        }

        if (portBrand == PortBrand.Image)
        {
            value = new Image(new ImageTorque.Buffers.PixelBuffer<ImageTorque.Pixels.L8>(2, 2));
        }

        IPort runtimePort = portBrand switch
        {
            PortBrand.Boolean => new BooleanPort("P1", PortDirection.Input, (bool)value!),
            PortBrand.Numeric => new NumericPort("P2", PortDirection.Output, Convert.ToDouble(value)),
            PortBrand.String => new StringPort("P3", PortDirection.Input, (string)value!),
            PortBrand.Folder => new FolderPort("P4", PortDirection.Output, (string)value!),
            PortBrand.Enum => new EnumPort("P5", PortDirection.Input, (PortDirection)value!),
            PortBrand.Rectangle => new RectanglePort("P6", PortDirection.Input, (ImageTorque.Rectangle)value!),
            PortBrand.Image => new ImagePort("P7", PortDirection.Input, (Image)value!),
            _ => throw new NotImplementedException(),
        };

        // Act
        PortModel result = service.FromRuntime(runtimePort);

        // Assert
        Assert.Equal(runtimePort.Id, result.Id);
        Assert.Equal(runtimePort.Name, result.Name);
        Assert.Equal(runtimePort.Direction, result.Direction);
        Assert.Equal(runtimePort.Brand, result.Brand);
        Assert.Equal(runtimePort.IsConnected, result.IsConnected);

        switch (portBrand)
        {
            case PortBrand.Boolean:
                Assert.Equal(((BooleanPort)runtimePort).Value, result.Value);
                break;
            case PortBrand.Numeric:
                Assert.Equal(((NumericPort)runtimePort).Value, Convert.ToDouble(result.Value));
                break;
            case PortBrand.String:
                Assert.Equal(((StringPort)runtimePort).Value, result.Value);
                break;
            case PortBrand.Folder:
                Assert.Equal(((FolderPort)runtimePort).Value, result.Value);
                break;
            case PortBrand.Enum:
                Assert.Equal(((EnumPort)runtimePort).Value, result.Value);
                break;
            case PortBrand.Rectangle:
                Assert.Equal(((RectanglePort)runtimePort).Value, result.Value);
                break;
            case PortBrand.Image:
                Assert.Equal(new CacheImage
                {
                    Meta = new ImageMeta
                    {
                        Width = 2,
                        Height = 2,
                        PixelFormat = PixelFormat.Mono8
                    },
                    OriginalImage = new Image((Image)value!)
                }, result.Value);
                break;
        }

    }
}
