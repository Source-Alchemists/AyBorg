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

using AyBorg.Communication;
using AyBorg.Communication.MQTT;
using AyBorg.Types;

using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.MQTT;

public sealed class MqttClientProvider : IDeviceProvider
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<MqttClientProvider> _logger;
    private readonly ICommunicationStateProvider _communicationStateProvider;
    private readonly IMqttClientProviderFactory _clientProviderFactory;

    public string Name => "MQTT Clients";

    public string Prefix => "AyBM";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultDeviceCategories.Communication, "MQTT" };

    public bool CanCreate => true;

    public MqttClientProvider(ILogger<MqttClientProvider> logger, ILoggerFactory loggerFactory, ICommunicationStateProvider communicationStateProvider)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _communicationStateProvider = communicationStateProvider;
        _clientProviderFactory = new MqttClientProviderFactory();
    }

    public async ValueTask<IDevice> CreateAsync(string id)
    {
        var client = new MqttClient(_loggerFactory.CreateLogger<MqttClient>(), _clientProviderFactory, _communicationStateProvider, id);
        _logger.LogTrace((int)EventLogType.Plugin, "Added MQTT client '{id}'", id);
        return await ValueTask.FromResult(client);
    }
}
