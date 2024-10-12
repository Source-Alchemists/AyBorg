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
using AyBorg.Types.Ports;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

namespace AyBorg.Plugins.ImageTorque.Tests;

public class ImageLoadTests : IDisposable
{
    private static readonly NullLogger<ImageLoad> s_logger = new();
    private readonly Mock<IEnvironment> _mockEnvironment = new();
    private readonly ImageLoad _plugin;
    private bool _disposedValue;

    public ImageLoadTests()
    {
        _plugin = new ImageLoad(s_logger, _mockEnvironment.Object);
    }

    [Fact]
    public async Task Test_TryRunAsync()
    {
        // Arrange
        _mockEnvironment.Setup(m => m.StorageLocation).Returns("./");
        var folderPort = (FolderPort)_plugin.Ports.First(p => p.Name.Equals("Folder"));
        var imagePort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Image"));
        folderPort.Value = "resources";

        // Act
        bool result = await _plugin.TryRunAsync(CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.NotNull(imagePort.Value);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue && disposing)
        {
            Task.Delay(10).Wait(); // Give some time to finished the background task.
            _plugin?.Dispose();
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
