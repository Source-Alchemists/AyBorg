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

using AyBorg.Agent.Runtime;
using AyBorg.Runtime;
using AyBorg.Runtime.Projects;

using Microsoft.Extensions.Logging.Abstractions;

namespace AyBorg.Agent.Services.Tests;

public class EngineFactoryTests
{
    private static readonly NullLogger<EngineFactory> s_logger = new();
    private static readonly NullLoggerFactory s_nullLoggerFactory = new();
    private readonly EngineFactory _factory;

    public EngineFactoryTests()
    {
        _factory = new EngineFactory(s_logger, s_nullLoggerFactory);
    }

    [Theory]
    [InlineData(EngineExecutionType.SingleRun)]
    [InlineData(EngineExecutionType.ContinuousRun)]
    public void Test_CreateEngine(EngineExecutionType engineExecutionType)
    {
        // Arrange
        var project = new Project();

        // Act
        using IEngine engine = _factory.CreateEngine(project, engineExecutionType);

        // Assert
        Assert.NotNull(engine);
    }
}
