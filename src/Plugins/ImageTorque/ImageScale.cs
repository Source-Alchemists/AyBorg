using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using ImageTorque;

namespace AyBorg.Plugins.ImageTorque;

public sealed class ImageScale : IStepBody, IDisposable
{
    private readonly ImagePort _imagePort = new("Image", PortDirection.Input, null!);
    private readonly ImagePort _scaledImagePort = new("Scaled image", PortDirection.Output, null!);
    private readonly NumericPort _widthPort = new("Width", PortDirection.Output, 0);
    private readonly NumericPort _heightPort = new("Height", PortDirection.Output, 0);
    private readonly NumericPort _scalePort = new("Scale factor", PortDirection.Input, 0.5d, 0.01d, 2d);
    private bool _disposedValue;

    public string Name => "Image.Scale";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.ImageProcessing };

    public IReadOnlyCollection<IPort> Ports { get; }

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

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _scaledImagePort.Value?.Dispose();
        Image sourceImage = _imagePort.Value;
        if (_scalePort.Value.Equals(1d))
        {
            _scaledImagePort.Value = sourceImage;
            return ValueTask.FromResult(true);
        }

        int w = (int)(sourceImage.Width * _scalePort.Value);
        int h = (int)(sourceImage.Height * _scalePort.Value);
        _scaledImagePort.Value = sourceImage.Resize(w, h);
        _widthPort.Value = w;
        _heightPort.Value = h;
        return ValueTask.FromResult(true);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue && disposing)
        {
            _scaledImagePort?.Dispose();
            _disposedValue = true;
        }
    }
}
