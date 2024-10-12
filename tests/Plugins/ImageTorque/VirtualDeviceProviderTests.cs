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
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Plugins.ImageTorque.Tests;

public class VirtualDeviceProviderTests
{
    private static readonly NullLogger<VirtualDeviceProvider> s_nullLogger = new();
    private static readonly NullLoggerFactory s_nullLoggerFactory = new();
    private readonly Mock<IEnvironment> _environmentMock = new();
    private readonly VirtualDeviceProvider _plugin;

    public VirtualDeviceProviderTests()
    {
        _plugin = new VirtualDeviceProvider(s_nullLogger, s_nullLoggerFactory, _environmentMock.Object);
    }

    [Fact]
    public void Test_Properties()
    {
        // Assert
        Assert.Equal("AyBV", _plugin.Prefix);
        Assert.True(_plugin.CanCreate);
        Assert.Equal("Virtual Devices", _plugin.Name);
        Assert.Equal(2, _plugin.Categories.Count);
        Assert.Contains("Camera", _plugin.Categories);
        Assert.Contains("Virtual Device", _plugin.Categories);
    }

    [Fact]
    public async Task Test_CreateAsync()
    {
        // Act
        IDevice result = await _plugin.CreateAsync("123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result.Id);
        Assert.Equal("Virtual Device (123)", result.Name);
    }
}
