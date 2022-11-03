using Atomy.SDK;
using Atomy.SDK.ImageProcessing;
using Atomy.SDK.Ports;

namespace Atomy.Plugins.Base;

public sealed class ImageScale : IStepBody, IDisposable
{
    private readonly ImagePort _imagePort = new ImagePort("Image", PortDirection.Input, null!);
    private readonly ImagePort _scaledImagePort = new ImagePort("Scaled image", PortDirection.Output, null!);
    private readonly NumericPort _widthPort = new NumericPort("Width", PortDirection.Output, 0);
    private readonly NumericPort _heightPort = new NumericPort("Height", PortDirection.Output, 0);
    private readonly NumericPort _scalePort = new NumericPort("Scale factor", PortDirection.Input, 0.5d, 0.01d, 2d);
    private bool disposedValue;

    public string DefaultName => "Image.Scale";

    public IEnumerable<IPort> Ports { get; }

    public ImageScale()
    {
        Ports = new List<IPort>
        {
            _imagePort,
            _scaledImagePort,
            _widthPort,
            _heightPort,
            _scalePort
        };
    }

    public Task<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _scaledImagePort.Value?.Dispose();
        var sourceImage = _imagePort.Value;
        if (_scalePort.Value.Equals(1m))
        {
            _scaledImagePort.Value = sourceImage;
            return Task.FromResult(true);
        }

        int w = (int)(sourceImage.Width * _scalePort.Value);
        int h = (int)(sourceImage.Height * _scalePort.Value);
        _scaledImagePort.Value = sourceImage.Resize(w, h);
        _widthPort.Value = w;
        _heightPort.Value = h;
        return Task.FromResult(true);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _scaledImagePort?.Dispose();
            }
            disposedValue = true;
        }
    }
}