using AyBorg.SDK.Common;
using AyBorg.SDK.Common.ImageAcquisition;
using AyBorg.SDK.Common.Ports;
using ImageTorque;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.ImageTorque;

public sealed class VirtualDevice : ICameraDevice
{
    private readonly ILogger<VirtualDevice> _logger;
    private readonly IEnvironment _environment;
    private readonly FolderPort _folderPort = new("Folder", PortDirection.Input, string.Empty);
    private static readonly string[] s_supportedFileTypes = new[] { ".jpg", ".jpeg", ".png", ".bmp" };
    private int _imageIndex = 0;
    private Task<ImageContainer>? _preloadTask;
    private string _lastFolderPath = string.Empty;
    private ImageContainer? _lastImageContainer;
    private long _imageCounter;

    public string Id { get; }

    public string Manufacturer => "Source Alchemists";

    public bool IsConnected { get; private set; }

    public IReadOnlyCollection<IPort> Ports { get; }

    public string Name { get; }

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultDeviceCategories.Camera, "Virtual Device" };

    public VirtualDevice(ILogger<VirtualDevice> logger, IEnvironment environment, string id)
    {
        _logger = logger;
        _environment = environment;
        Id = id;
        Name = $"Virtual Device ({id})";

        Ports = new List<IPort> { _folderPort };
    }

    public async ValueTask<ImageContainer> AcquisitionAsync(CancellationToken cancellationToken)
    {
        _lastImageContainer?.Dispose();
        if (_preloadTask == null)
        {
            _preloadTask = PreloadImageAsync();
        }
        else if (!string.IsNullOrEmpty(_lastFolderPath) && !_lastFolderPath.Equals(_folderPort.Value, StringComparison.InvariantCultureIgnoreCase))
        {
            // File path changed while preloading a image
            await _preloadTask;
            _preloadTask = PreloadImageAsync();
        }

        _lastFolderPath = _folderPort.Value;

        _lastImageContainer = await _preloadTask;
        _preloadTask.Dispose();
        _preloadTask = PreloadImageAsync();
        return _lastImageContainer;
    }
    public ValueTask<bool> TryConnectAsync()
    {
        try
        {
            _preloadTask?.Dispose();
            _preloadTask = PreloadImageAsync();
            IsConnected = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), ex, "Failed to connect to virtual device");
            IsConnected = false;
        }

        return ValueTask.FromResult(IsConnected);
    }
    public ValueTask<bool> TryDisconnectAsync()
    {
        try
        {
            _preloadTask?.Dispose();
            IsConnected = false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), ex, "Failed to disconnect from virtual device");
            IsConnected = true;
        }

        return ValueTask.FromResult(!IsConnected);
    }
    public async ValueTask<bool> TryUpdate(IReadOnlyCollection<IPort> ports)
    {
        bool prevConnected = IsConnected;
        if (IsConnected && !await TryDisconnectAsync())
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Failed disconnecting virtual device");
            return false;
        }

        foreach (IPort port in ports)
        {
            IPort? targetPort = Ports.FirstOrDefault(p => p.Id.Equals(port.Id) && p.Brand.Equals(port.Brand));
            if (targetPort == null)
            {
                _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Port {PortId} not found", port.Id);
                continue;
            }

            targetPort.UpdateValue(port);
        }

        if (prevConnected && !await TryConnectAsync())
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Failed connecting virtual device");
            return false;
        }

        _logger.LogTrace(new EventId((int)EventLogType.Plugin), "Updated virtual device");
        return true;
    }

    private Task<ImageContainer> PreloadImageAsync()
    {
        return Task.Factory.StartNew(() =>
        {
            string absolutPath = Path.GetFullPath($"{_environment.StorageLocation}{_folderPort.Value}");
            string[] files = Directory.GetFiles(absolutPath);
            IEnumerable<string> supportedFiles = files.Where(f => s_supportedFileTypes.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase));
            string[] imageFileNames = supportedFiles.ToArray();
            if (imageFileNames.Length == 0)
            {
                _logger.LogWarning(new EventId((int)EventLogType.Plugin), "No images found in folder {folder}", _folderPort.Value);
                return null!;
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
            return new ImageContainer(image, _imageCounter++, imageFileName);
        });
    }
}
