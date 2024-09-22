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

using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Plugins.ImageTorque.Tests;

public class VirtualDeviceTests : IDisposable
{
    private static readonly NullLogger<VirtualDevice> s_nullLogger = new();
    private readonly Mock<IEnvironment> _environmentMock = new();
    private readonly VirtualDevice _plugin;
    private bool _isDisposed = false;

    public VirtualDeviceTests()
    {
        _environmentMock.Setup(m => m.StorageLocation).Returns("./");
        _plugin = new VirtualDevice(s_nullLogger, _environmentMock.Object, "123");
    }

    [Fact]
    public async Task Test_TryConnectAsync()
    {
        // Arrange
        var folderPort = (FolderPort)_plugin.Ports.First(p => p.Name.Equals("Folder"));
        folderPort.Value = "resources";

        // Act
        bool result = await _plugin.TryConnectAsync();

        // Assert
        Assert.True(result);
        Assert.True(_plugin.IsConnected);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public async Task Test_AcquisitionAsync(long acquisitions)
    {
        // Arrange
        var folderPort = (FolderPort)_plugin.Ports.First(p => p.Name.Equals("Folder"));
        folderPort.Value = "resources";

        // Act
        long resultIndex = 0;
        for (long i = 0; i < acquisitions; i++)
        {
            ImageContainer result = await _plugin.AcquisitionAsync(CancellationToken.None);
            resultIndex = result.Index;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Image);
            Assert.NotEqual(string.Empty, result.AdditionInfo);
        }

        Assert.Equal(acquisitions-1, resultIndex);
    }

    [Fact]
    public async Task Test_TryDisconnectAsync()
    {
        // Act
        bool result = await _plugin.TryDisconnectAsync();

        // Assert
        Assert.True(result);
        Assert.False(_plugin.IsConnected);
    }

    [Fact]
    public async Task Test_TryUpdateAsync()
    {
        // Arrange
        var folderPort = (FolderPort)_plugin.Ports.First(p => p.Name.Equals("Folder"));
        folderPort.Value = "resources2";

        var folderPortCopy = new FolderPort(folderPort)
        {
            Value = "resources"
        };

        // Act
        bool result = await _plugin.TryUpdateAsync(new List<IPort> { folderPortCopy });

        // Assert
        Assert.True(result);
        Assert.Equal("resources", folderPort.Value);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
        if (isDisposing && !_isDisposed)
        {
            _plugin?.Dispose();
            _isDisposed = true;
        }
    }
}
