using Atomy.SDK.ImageProcessing;
using Atomy.SDK.ImageProcessing.Shapes;
using Atomy.SDK.Common.Ports;
using Microsoft.Extensions.Logging;
using Atomy.SDK.Common;

namespace Atomy.Plugins.Base;

public sealed class ImageCrop : IStepBody, IDisposable
{
    private readonly ILogger<ImageCrop> _logger;
    private readonly ImagePort _inputImagePort = new ImagePort("Image", PortDirection.Input, null!);
    private readonly RectanglePort _cropRectanglePort = new RectanglePort("Region", PortDirection.Input, new Rectangle());
    private readonly ImagePort _outputImagePort = new ImagePort("Cropped image", PortDirection.Output, null!);
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

    public Task<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _outputImagePort.Value?.Dispose();
        if(_cropRectanglePort.Value.Width <= 0 || _cropRectanglePort.Value.Height <= 0)
        {
            _logger.LogWarning("Invalid crop region.");
            return Task.FromResult(false);
        }
        
        _outputImagePort.Value = _inputImagePort.Value.Crop(_cropRectanglePort.Value);
        return Task.FromResult(true);
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