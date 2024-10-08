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
using ImageTorque;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.ImageTorque;

public sealed class ImageSave : IStepBody
{
    private readonly ILogger<ImageSave> _logger;
    private readonly IEnvironment _environment;
    private readonly ImagePort _imagePort = new("Image", PortDirection.Input, null!);
    private readonly StringPort _fileNamePrefixPort = new("File name prefix", PortDirection.Input, string.Empty);
    private readonly StringPort _inputFileNamePort = new("File name", PortDirection.Input, "{Guid}");
    private readonly StringPort _fileNameSuffixPort = new("File name suffix", PortDirection.Input, "_{DateTime}");
    private readonly FolderPort _folderPort = new("Folder", PortDirection.Input, string.Empty);
    private readonly StringPort _outputFileNamePort = new("File name", PortDirection.Output, string.Empty);

    public string Name => "Image.Save";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.ImageProcessing };

    public IReadOnlyCollection<IPort> Ports { get; }

    public ImageSave(ILogger<ImageSave> logger, IEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
        Ports =
        [
            _imagePort,
            _folderPort,
            _fileNamePrefixPort,
            _inputFileNamePort,
            _fileNameSuffixPort,
            _outputFileNamePort
        ];
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        string resultFileName = $"{ReplacePlaceHolder(_fileNamePrefixPort.Value)}{ReplacePlaceHolder(_inputFileNamePort.Value)}{ReplacePlaceHolder(_fileNameSuffixPort.Value)}.png";
        string resultFilePath = Path.Combine($"{_environment.StorageLocation}{_folderPort.Value}", resultFileName);
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(new EventId((int)EventLogType.Result), "Saving image to {resultFilePath}", resultFilePath);
        }
        _imagePort.Value.Save(resultFilePath);
        _outputFileNamePort.Value = resultFileName;
        return ValueTask.FromResult(true);
    }

    private static string ReplacePlaceHolder(string value)
    {
        string result = Path.GetFileNameWithoutExtension(value);
        result = result.Replace("{Guid}", Guid.NewGuid().ToString());
        result = result.Replace("{DateTime}", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        result = result.Replace("{Date}", DateTime.UtcNow.ToString("yyyyMMdd"));
        result = result.Replace("{Time}", DateTime.UtcNow.ToString("HHmmss"));
        result = result.Replace("{Year}", DateTime.UtcNow.ToString("yyyy"));
        result = result.Replace("{Month}", DateTime.UtcNow.ToString("MM"));
        result = result.Replace("{Day}", DateTime.UtcNow.ToString("dd"));
        result = result.Replace("{Hour}", DateTime.UtcNow.ToString("HH"));
        result = result.Replace("{Minute}", DateTime.UtcNow.ToString("mm"));
        result = result.Replace("{Second}", DateTime.UtcNow.ToString("ss"));
        result = result.Replace("{Millisecond}", DateTime.UtcNow.ToString("fff"));
        return result;
    }
}
