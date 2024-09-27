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

namespace AyBorg.Agent.Services;

internal sealed class StorageService : IStorageService
{
    private const string VIRTUAL_ROOT_PATH = "/";
    private readonly ILogger<StorageService> _logger;
    private readonly IEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="environment">The environment.</param>
    public StorageService(ILogger<StorageService> logger, IEnvironment environment)
    {
        _logger = logger;
        _environment = environment;

        if (!Directory.Exists(_environment.StorageLocation))
        {
            try
            {
                Directory.CreateDirectory(_environment.StorageLocation);
                _logger.LogInformation("Created storage directory at {StorageLocation}", _environment.StorageLocation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create storage directory at {StorageLocation}", _environment.StorageLocation);
            }
        }

        _logger.LogTrace("Storage directory: {StorageLocation}", _environment.StorageLocation);
    }

    /// <summary>
    /// Gets the directories.
    /// </summary>
    /// <param name="virtualPath">The virtual path.</param>
    /// <returns></returns>
    public IEnumerable<string> GetDirectories(string virtualPath)
    {
        string realPath;

        if (virtualPath.Equals(VIRTUAL_ROOT_PATH)
            || virtualPath.Equals(Path.DirectorySeparatorChar.ToString())
            || virtualPath.Equals(Path.AltDirectorySeparatorChar.ToString()))
        {
            realPath = _environment.StorageLocation;
        }
        else
        {
            if (virtualPath.StartsWith(VIRTUAL_ROOT_PATH)
                || virtualPath.StartsWith(Path.DirectorySeparatorChar.ToString())
                || virtualPath.StartsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                virtualPath = virtualPath[1..];
            }

            virtualPath = virtualPath.Replace("..", string.Empty);

            realPath = Path.Combine(_environment.StorageLocation, virtualPath);
        }

        if (Directory.Exists(realPath))
        {
            foreach (string dir in Directory.GetDirectories(realPath))
            {
                string virtualSubPath = dir.Replace(_environment.StorageLocation, VIRTUAL_ROOT_PATH);
                virtualSubPath = virtualSubPath.Replace("\\", "/");
                virtualSubPath = virtualSubPath.Replace("//", "/");
                yield return virtualSubPath;
            }
        }
        else
        {
            yield return VIRTUAL_ROOT_PATH;
        }
    }
}
