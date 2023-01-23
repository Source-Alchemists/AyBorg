using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using ImageTorque;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.ImageTorque;

public sealed class ImageLoad : IStepBody, IDisposable
{
    private readonly ILogger<ImageLoad> _logger;
    private readonly IEnvironment _environment;
    private readonly FolderPort _folderPort = new("Folder", PortDirection.Input, string.Empty);
    private readonly ImagePort _imagePort = new("Image", PortDirection.Output, null!);
    private readonly StringPort _filePathPort = new("File path", PortDirection.Output, string.Empty);
    private readonly string[] _supportedFileTypes = new[] { ".jpg", ".jpeg", ".png", ".bmp" };
    private int _imageIndex = 0;
    private bool _disposedValue;
    private Task<KeyValuePair<string, Image>>? _preloadTask;
    private string _lastFolderPath = string.Empty;

    public string DefaultName => "Image.Load";

    public IEnumerable<string> Categories { get; } = new List<string> { DefaultStepCategories.ImageProcessing, DefaultStepCategories.Simulation };

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

    public async ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _imagePort.Value?.Dispose();
        if (_preloadTask == null)
        {
            _preloadTask = PreloadImage();
        }
        else if (!string.IsNullOrEmpty(_lastFolderPath) && !_lastFolderPath.Equals(_folderPort.Value, StringComparison.InvariantCultureIgnoreCase))
        {
            // File path changed while preloading a image
            await _preloadTask;
            _preloadTask = PreloadImage();
        }

        _lastFolderPath = _folderPort.Value;

        KeyValuePair<string, Image> result = await _preloadTask;
        _imagePort.Value = result.Value;
        _filePathPort.Value = result.Key;
        _preloadTask.Dispose();
        _preloadTask = PreloadImage();

        return true;
    }

    private Task<KeyValuePair<string, Image>> PreloadImage()
    {
        return Task.Factory.StartNew(() =>
        {
            string absolutPath = Path.GetFullPath($"{_environment.StorageLocation}{_folderPort.Value}");
            string[] files = Directory.GetFiles(absolutPath);
            IEnumerable<string> supportedFiles = files.Where(f => _supportedFileTypes.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase));
            string[] imageFileNames = supportedFiles.ToArray();
            if (imageFileNames.Length == 0)
            {
                _logger.LogWarning(new EventId((int)EventLogType.PluginState), "No images found in folder {folder}", _folderPort.Value);
                return new KeyValuePair<string, Image>(string.Empty, null!);
            }

            if (_imageIndex >= imageFileNames.Length)
            {
                _imageIndex = 0;
            }
            string imageFileName = imageFileNames![_imageIndex];
            var image = Image.Load(imageFileName);
            imageFileName = imageFileName.Replace(_environment.StorageLocation, string.Empty);
            imageFileName = imageFileName.Replace('\\', '/');
            _imageIndex++;
            return new KeyValuePair<string, Image>(imageFileName, image);
        });
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue && disposing)
        {
            _imagePort.Value?.Dispose();
            _preloadTask?.Wait();
            _preloadTask?.Dispose();
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
