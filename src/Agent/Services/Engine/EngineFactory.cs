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

namespace AyBorg.Agent.Services;

internal sealed class EngineFactory : IEngineFactory
{
    private readonly ILogger<EngineFactory> _logger;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="EngineFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public EngineFactory(ILogger<EngineFactory> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Creates the engine.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <param name="executionType">Type of the execution.</param>
    /// <returns></returns>
    public IEngine CreateEngine(Project project, EngineExecutionType executionType)
    {
        _logger.LogTrace("Creating engine with execution type [{executionType}].", executionType);
        ILogger<Engine> engineLogger = _loggerFactory.CreateLogger<Engine>();
        return new Engine(engineLogger, _loggerFactory, project, executionType);
    }
}
