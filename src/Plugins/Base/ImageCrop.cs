using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.ImageProcessing;
using AyBorg.SDK.ImageProcessing.Shapes;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base;

public sealed class ImageCrop : IStepBody, IDisposable
{
    private readonly ILogger<ImageCrop> _logger;
    private readonly ImagePort _inputImagePort = new("Image", PortDirection.Input, null!);
    private readonly RectanglePort _cropRectanglePort = new("Region", PortDirection.Input, new Rectangle());
    private readonly ImagePort _outputImagePort = new("Cropped image", PortDirection.Output, null!);
    private bool disposedValue;

    public string DefaultName => "Image.Crop";

    public IEnumerable<IPort> Ports { get; }

    public ImageCrop(ILogger<ImageCrop> logger)
    {
        _logger = logger;
        Ports = new IPort[]
        {
            _inputImagePort,
            _cropRectanglePort,
            _outputImagePort
        };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _outputImagePort.Value?.Dispose();
        if (_cropRectanglePort.Value.Width <= 0 || _cropRectanglePort.Value.Height <= 0)
        {
            _logger.LogWarning("Invalid crop region.");
            return ValueTask.FromResult(false);
        }

        _outputImagePort.Value = _inputImagePort.Value.Crop(_cropRectanglePort.Value);
        return ValueTask.FromResult(true);
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _outputImagePort.Value?.Dispose();
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