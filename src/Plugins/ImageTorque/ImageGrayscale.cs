using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using ImageTorque;

namespace AyBorg.Plugins.ImageTorque;

public sealed class ImageGrayscale : IStepBody, IDisposable
{
    private readonly ImagePort _imagePort = new("Image", PortDirection.Input, null!);
    private readonly ImagePort _grayscaleImagePort = new("Grayscale image", PortDirection.Output, null!);
    private bool _disposedValue;

    public string DefaultName => "Image.Grayscale";

    public IEnumerable<string> Categories { get; } = new List<string> { DefaultStepCategories.ImageProcessing };

    public IEnumerable<IPort> Ports { get; }

    public ImageGrayscale()
    {
        Ports = new List<IPort>
        {
            _imagePort,
            _grayscaleImagePort
        };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _grayscaleImagePort.Value?.Dispose();
        _grayscaleImagePort.Value = _imagePort.Value.ToGrayscale();
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
            _grayscaleImagePort?.Dispose();
            _disposedValue = true;
        }
    }
}
