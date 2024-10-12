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

using AyBorg.Agent.Services;
using AyBorg.Runtime;
using AyBorg.Runtime.Projects;
using AyBorg.Types.Models;
using AyBorg.Types.Ports;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AyBorg.Agent.Tests.Services;

public class CacheServiceTests
{
    private readonly Mock<IRuntimeMapper> _mockRuntimeMapper = new();
    private readonly IConfiguration _configuration;
    private readonly CacheService _service;

    public CacheServiceTests()
    {
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(
            initialData: new List<KeyValuePair<string, string>> {
                new("AyBorg:Cache:MaxSeconds", "10"),
                new("AyBorg:Cache:MaxIterations", "5")
            }!).Build();

        _service = new CacheService(_mockRuntimeMapper.Object, _configuration);
    }

    [Fact]
    public void Test_CreateCache()
    {
        // Arrange
        var iterationId = Guid.NewGuid();
        var project = new Project
        {
            Steps = new List<IStepProxy>
            {
                CreateStepProxyMock("S1", 1, new List<IPort> {
                    CreatePortMock("P1", PortDirection.Input, true, PortBrand.Numeric).Object,
                    CreatePortMock("P2", PortDirection.Input, false, PortBrand.String).Object,
                    CreatePortMock("P3", PortDirection.Output, true, PortBrand.String).Object
                }).Object
            }
        };

        _mockRuntimeMapper.Setup(m => m.FromRuntime(It.IsAny<IStepProxy>(), true)).Returns(new StepModel
        {
            Ports = new List<PortModel> {
                new() { Name = "P1", Direction = PortDirection.Input, IsConnected = true }
            }
        });

        // Act
        _service.CreateCache(iterationId, project);

        // Assert
        Assert.Equal(4, _service.CacheSize);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Test_GetOrCreateStepEntry(bool isCached)
    {
        // Arrange
        var iterationId = Guid.NewGuid();
        Mock<IStepProxy> stepProxyMock = CreateStepProxyMock("S1", 1, new List<IPort> {
                    CreatePortMock("P1", PortDirection.Input, true, PortBrand.Numeric).Object,
                    CreatePortMock("P2", PortDirection.Input, false, PortBrand.String).Object,
                    CreatePortMock("P3", PortDirection.Output, true, PortBrand.String).Object
                });
        IStepProxy stepProxy = stepProxyMock.Object;
        var project = new Project
        {
            Steps = new List<IStepProxy> { stepProxy }
        };

        _mockRuntimeMapper.Setup(m => m.FromRuntime(It.Is<IStepProxy>(s => s.Id.Equals(stepProxy.Id)), true)).Returns(new StepModel
        {
            ExecutionTimeMs = 42,
            Ports = new List<PortModel> {
                new() { Name = "P1", Direction = PortDirection.Input, IsConnected = true }
            }
        });

        if (isCached)
        {
            _service.CreateCache(iterationId, project);
        }

        // Act
        StepModel resultStep = _service.GetOrCreateStepEntry(iterationId, stepProxyMock.Object);

        // Assert
        Assert.Equal(42, resultStep.ExecutionTimeMs);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Test_GetOrCreatePortEntry(bool isCached)
    {
        // Arrange
        var iterationId = Guid.NewGuid();
        Mock<IStepProxy> stepProxyMock = CreateStepProxyMock("S1", 1, new List<IPort> {
                    CreatePortMock("P1", PortDirection.Input, true, PortBrand.Numeric).Object,
                    CreatePortMock("P2", PortDirection.Input, false, PortBrand.String).Object,
                    CreatePortMock("P3", PortDirection.Output, true, PortBrand.String).Object
                });
        IStepProxy stepProxy = stepProxyMock.Object;
        var project = new Project
        {
            Steps = new List<IStepProxy> { stepProxy }
        };

        _mockRuntimeMapper.Setup(m => m.FromRuntime(It.Is<IStepProxy>(s => s.Id.Equals(stepProxy.Id)), true)).Returns(new StepModel
        {
            Ports = new List<PortModel> {
                new PortModel { Name = "P1", Direction = PortDirection.Input, IsConnected = true, Value = "123" }
            }
        });

        _mockRuntimeMapper.Setup(m => m.FromRuntime(It.IsAny<IPort>())).Returns(new PortModel
        {
            Value = "123"
        });

        if (isCached)
        {
            _service.CreateCache(iterationId, project);
        }

        // Act
        PortModel resultPort = _service.GetOrCreatePortEntry(iterationId, stepProxy.Ports.First(p => p.Name.Equals("P1")));

        // Assert
        Assert.Equal("123", resultPort.Value);
    }

    private static Mock<IStepProxy> CreateStepProxyMock(string name, long executionTimeMs, IEnumerable<IPort> ports)
    {
        var mockStepProxy = new Mock<IStepProxy>();
        mockStepProxy.Setup(s => s.Id).Returns(Guid.NewGuid());
        mockStepProxy.Setup(s => s.Name).Returns(name);
        mockStepProxy.Setup(s => s.ExecutionTimeMs).Returns(executionTimeMs);
        mockStepProxy.Setup(s => s.Ports).Returns(ports);
        return mockStepProxy;
    }

    private static Mock<IPort> CreatePortMock(string name, PortDirection portDirection, bool isConnected, PortBrand portBrand)
    {
        var mockPort = new Mock<IPort>();
        mockPort.Setup(p => p.Id).Returns(Guid.NewGuid());
        mockPort.Setup(p => p.Name).Returns(name);
        mockPort.Setup(p => p.Direction).Returns(portDirection);
        mockPort.Setup(p => p.IsConnected).Returns(isConnected);
        mockPort.Setup(p => p.Brand).Returns(portBrand);
        return mockPort;
    }
}
