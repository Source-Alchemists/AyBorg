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

using AyBorg.Types;

using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.ImageTorque;

public sealed class VirtualDeviceProvider : IDeviceProvider
{
    private readonly ILogger<VirtualDeviceProvider> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IEnvironment _environment;

    public string Prefix => "AyBV";

    public bool CanCreate => true;

    public string Name => "Virtual Devices";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultDeviceCategories.Camera, "Virtual Device" };

    public VirtualDeviceProvider(ILogger<VirtualDeviceProvider> logger, ILoggerFactory loggerFactory, IEnvironment environment)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _environment = environment;
    }

    public async ValueTask<IDevice> CreateAsync(string id)
    {
        var device = new VirtualDevice(_loggerFactory.CreateLogger<VirtualDevice>(), _environment, id);
        _logger.LogTrace((int)EventLogType.Plugin, "Added virtual device '{id}'", id);
        return await ValueTask.FromResult(device);
    }
}
