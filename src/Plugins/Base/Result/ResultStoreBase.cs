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
using System.Collections.Immutable;

using AyBorg.Runtime;
using AyBorg.Types;
using AyBorg.Types.Models;
using AyBorg.Types.Ports;
using AyBorg.Types.Result;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base;

public abstract class ResultStoreBase : IStepBody
{
    protected readonly ILogger<ResultStoreBase> _logger;
    protected readonly IResultStorageProvider _resultStorageProvider;
    protected readonly IRuntimeMapper _runtimeMapper;
    protected readonly StringPort _idPort = new("Id", PortDirection.Input, "topic/id");
    protected ImmutableList<IPort> _ports = ImmutableList.Create<IPort>();
    public IReadOnlyCollection<IPort> Ports => _ports;

    public abstract string Name { get; }

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.Result };

    protected ResultStoreBase(ILogger<ResultStoreBase> logger, IResultStorageProvider resultStorageProvider, IRuntimeMapper runtimeMapper)
    {
        _logger = logger;
        _resultStorageProvider = resultStorageProvider;
        _runtimeMapper = runtimeMapper;

        _ports = _ports.Add(_idPort);
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_idPort.Value))
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Failed to store result. Id is empty.");
            return ValueTask.FromResult(false);
        }

        PortModel portModel = Map();
        try
        {
            _resultStorageProvider.Add(new PortResult(
                _idPort.Value,
                portModel
            ));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), ex, "Failed to store result {portResultId}'.", _idPort.Value);
            return ValueTask.FromResult(false);
        }

        return ValueTask.FromResult(true);
    }

    protected abstract PortModel Map();
}
