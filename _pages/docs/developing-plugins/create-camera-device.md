---
title: Create a camera device
description: This guide is focused on developing camera device plugins for AyBorg, covering the essential public interface methods required for implementing a camera device. While the principles can be applied to different types of camera devices, the guide is targeted at general camera device development.
sidebar: developing-plugins
permalink: "/docs/developing-plugins/create-camera-device.html"
---

This guide is focused on developing camera device plugins for AyBorg, covering the essential public interface methods required for implementing a camera device. While the principles can be applied to different types of camera devices, the guide is targeted at general camera device development.

## Prerequisites

- Familiarity with C# programming language
- .NET 7+ installed on your machine
- An IDE (e.g., Visual Studio, JetBrains Rider, or Visual Studio Code)

## Import the necessary namespaces for camera devices

```csharp
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.ImageAcquisition;
using AyBorg.SDK.Common.Ports;
```

## Implementing camera device class

Create a class implementing `ICameraDevice`, encapsulating the functionalities of the camera device.

## Implementing device properties

Define properties essential for the device:

**Id:** A unique identifier for the device.<br/>
**Manufacturer:** The manufacturer's name.<br/>
**IsConnected:** The connection status.<br/>
**Ports:** The collection of the device's ports.<br/>
**Name:** The device's name.<br/>
**Categories:** The collection of categories associated with the device.<br/>

## Handling image acquisition

`AcquisitionAsync`: A method that retrieves an image asynchronously according to the device's internal logic.

## Connection management

These methods are responsible for managing the device's connection state:

`TryConnectAsync`: A method that attempts to connect to the device, handling success or failure appropriately.
`TryDisconnectAsync`: A method that attempts to disconnect from the device, handling success or failure appropriately.
Port Management
`TryUpdateAsync`: A method to update the values of ports, handling synchronization with existing ports.

## Example implementation

```csharp
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.ImageAcquisition;
using AyBorg.SDK.Common.Ports;
using ImageTorque;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.ImageTorque;

public sealed class VirtualDevice : ICameraDevice, IDisposable
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
    private bool _isDisposed = false;

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

    public async ValueTask<bool> TryUpdateAsync(IReadOnlyCollection<IPort> ports)
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
            IEnumerable<string> supportedFiles = files.Where(f => s_supportedFileTypes.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase)).Order();
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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
        if(isDisposing && !_isDisposed)
        {
            _preloadTask?.Wait();
            _preloadTask?.Dispose();
            _lastImageContainer?.Dispose();
            _isDisposed = true;
        }
    }
}
```

## Conclusion

By following these guidelines and utilizing AyBorg's SDK, developers can create a versatile and robust camera device plugin for AyBorg. These principles can be adapted to different types of camera implementations.

## See also

- [Create a step]({{site.baseurl}}/docs/developing-plugins/create-step)
