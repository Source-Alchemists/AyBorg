using Autodroid.SDK.ImageProcessing;
using Autodroid.SDK.ImageProcessing.Encoding;
using Autodroid.SDK.Common.Ports;
using Microsoft.Extensions.Logging;
using Autodroid.SDK.Common;

namespace Autodroid.Plugins.Base;

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
    public string DefaultName => "Image.Save";

    public IEnumerable<IPort> Ports { get;}

    public ImageSave(ILogger<ImageSave> logger, IEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
        Ports = new List<IPort>
        {
            _imagePort,
            _folderPort,
            _fileNamePrefixPort,
            _inputFileNamePort,
            _fileNameSuffixPort,
            _outputFileNamePort
        };
    }

    public Task<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        var resultFileName = $"{ReplacePlaceHolder(_fileNamePrefixPort.Value)}{ReplacePlaceHolder(_inputFileNamePort.Value)}{ReplacePlaceHolder(_fileNameSuffixPort.Value)}.png";
        var resultFilePath = Path.Combine($"{_environment.StorageLocation}{_folderPort.Value}", resultFileName);
        _logger.LogTrace("Saving image to {resultFilePath}", resultFilePath);
        Image.Save(_imagePort.Value, resultFilePath, EncoderType.Png);
        _outputFileNamePort.Value = resultFileName;
        return Task.FromResult(true);
    }

    private static string ReplacePlaceHolder(string value)
    {
        var result = Path.GetFileNameWithoutExtension(value);
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