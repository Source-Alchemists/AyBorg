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
using AyBorg.Types;
using AyBorg.Types.Ports;
using Moq;

namespace AyBorg.Agent.Tests;

public static class MockHelper
{
    public static Mock<IStepProxy> CreateStepProxyMock(string name, int inputCount, int outputCount)
    {
        var stepMock = new Mock<IStepProxy>();
        stepMock.SetupGet(s => s.Id).Returns(Guid.NewGuid());
        stepMock.SetupGet(s => s.Name).Returns(name);
        stepMock.SetupGet(s => s.Links).Returns(new List<PortLink>());

        var ports = new List<IPort>();

        for (int index = 0; index < inputCount; index++)
        {
            var portMock = new Mock<IPort>();
            portMock.SetupGet(p => p.Id).Returns(Guid.NewGuid());
            portMock.SetupGet(p => p.Direction).Returns(PortDirection.Input);
            portMock.SetupGet(p => p.Name).Returns($"Input {index}");
            ports.Add(portMock.Object);
        }

        for (int index = 0; index < outputCount; index++)
        {
            var portMock = new Mock<IPort>();
            portMock.SetupGet(p => p.Id).Returns(Guid.NewGuid());
            portMock.SetupGet(p => p.Direction).Returns(PortDirection.Output);
            portMock.SetupGet(p => p.Name).Returns($"Output {index}");
            ports.Add(portMock.Object);
        }

        stepMock.Setup(s => s.Ports).Returns(ports);

        return stepMock;
    }

    public static Mock<IStepBody> CreateStepBodyMock(string name, int inputCount, int outputCount, bool successful = true)
    {
        var stepBodyMock = new Mock<IStepBody>();
        stepBodyMock.SetupGet(s => s.Name).Returns(name);
        stepBodyMock.Setup(s => s.TryRunAsync(It.IsAny<CancellationToken>())).ReturnsAsync(successful);

        var ports = new List<IPort>();

        for (int index = 0; index < inputCount; index++)
        {
            var portMock = new Mock<IPort>();
            portMock.SetupGet(p => p.Id).Returns(Guid.NewGuid());
            portMock.SetupGet(p => p.Direction).Returns(PortDirection.Input);
            portMock.SetupGet(p => p.Name).Returns($"Input {index}");
            ports.Add(portMock.Object);
        }

        for (int index = 0; index < outputCount; index++)
        {
            var portMock = new Mock<IPort>();
            portMock.SetupGet(p => p.Id).Returns(Guid.NewGuid());
            portMock.SetupGet(p => p.Direction).Returns(PortDirection.Output);
            portMock.SetupGet(p => p.Name).Returns($"Output {index}");
            ports.Add(portMock.Object);
        }

        stepBodyMock.Setup(s => s.Ports).Returns(ports);

        return stepBodyMock;
    }
}
