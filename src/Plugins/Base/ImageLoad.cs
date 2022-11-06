using Autodroid.SDK.ImageProcessing;
using Autodroid.SDK.Common.Ports;
using Microsoft.Extensions.Logging;
using Autodroid.SDK.Common;

namespace Autodroid.Plugins.Base;

public sealed class ImageLoad : IStepBody, IDisposable
{
    private readonly ILogger<ImageLoad> _logger;
    private readonly IEnvironment _environment;
    private readonly FolderPort _folderPort = new("Folder", PortDirection.Input, string.Empty);
    private readonly ImagePort _imagePort = new("Image", PortDirection.Output, null!);
    private readonly StringPort _filePathPort = new("File path", PortDirection.Output, string.Empty);
    private readonly string[] _supportedFileTypes = new[] { ".jpg", ".jpeg", ".png", ".bmp" };
    private int _imageIndex = 0;
    private bool disposedValue;
    private Task<KeyValuePair<string, Image>>? _preloadTask;

    public string DefaultName => "Image.Load";

    public IEnumerable<IPort> Ports { get; }

    public ImageLoad(ILogger<ImageLoad> logger, IEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
        Ports = new List<IPort>
        {
            _folderPort,
            _imagePort,
            _filePathPort
        };
    }

    public async Task<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _imagePort.Value?.Dispose();
        if (_preloadTask == null)
            _preloadTask = PreloadImage();
        var result = await _preloadTask;
        _imagePort.Value = result.Value;
        _filePathPort.Value = result.Key;
        _preloadTask.Dispose();
        _preloadTask = PreloadImage();

        return await Task.FromResult(true);
    }

    private Task<KeyValuePair<string, Image>> PreloadImage()
    {
        return Task.Factory.StartNew(() =>
        {
            var absolutPath = Path.GetFullPath($"{_environment.StorageLocation}{_folderPort.Value}");
            var files = Directory.GetFiles(absolutPath);
            var supportedFiles = files.Where(f => _supportedFileTypes.Contains(Path.GetExtension(f)));
            var imageFileNames = supportedFiles.ToArray();
            if (imageFileNames.Length == 0)
            {
                _logger.LogWarning("No images found in folder {folder}", _folderPort.Value);
                return new KeyValuePair<string, Image>(string.Empty, null!);
            }

            if (_imageIndex >= imageFileNames.Length)
            {
                _imageIndex = 0;
            }
            var imageFileName = imageFileNames![_imageIndex];
            var image = Image.Load(imageFileName);
            imageFileName = imageFileName.Replace(_environment.StorageLocation, string.Empty);
            imageFileName = imageFileName.Replace('\\', '/');
            _imageIndex++;
            return new KeyValuePair<string, Image>(imageFileName, image);
        });
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _imagePort.Value?.Dispose();
                _preloadTask?.Wait();
                _preloadTask?.Dispose();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}