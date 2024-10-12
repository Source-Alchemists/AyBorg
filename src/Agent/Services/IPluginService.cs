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

using AyBorg.Data.Agent;
using AyBorg.Runtime;
using AyBorg.Runtime.Devices;
using AyBorg.Types;

namespace AyBorg.Agent.Services;

public interface IPluginsService
{
    /// <summary>
    /// Gets the steps.
    /// </summary>
    IReadOnlyCollection<IStepProxy> Steps { get; }

    /// <summary>
    /// Gets the device providers.
    /// </summary>
    IReadOnlyCollection<IDeviceProviderProxy> DeviceProviders { get; }

    /// <summary>
    /// Loads this instance.
    /// </summary>
    ValueTask LoadAsync();

    /// <summary>
    /// Find plugin instance by step record.
    /// </summary>
    /// <param name="stepRecord">The step record.</param>
    /// <returns>Instance, else null.</returns>
    IStepProxy Find(StepRecord stepRecord);

    /// <summary>
    /// Finds the specified step.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <returns>Instance, else null.</returns>
    IStepProxy Find(Guid stepId);

    IDeviceProviderProxy? FindDeviceProvider(PluginMetaInfoRecord pluginMetaInfo);

    /// <summary>
    /// Creates new instance of step.
    /// </summary>
    /// <param name="stepBody">The step body.</param>
    /// <returns></returns>
    ValueTask<IStepProxy> CreateInstanceAsync(IStepBody stepBody);
}
