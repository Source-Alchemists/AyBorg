using Autodroid.SDK.ImageProcessing;
using Autodroid.SDK.Common.Ports;
using Autodroid.SDK.Common;

namespace Autodroid.Plugins.Base;

public sealed class ImageGrayscale : IStepBody, IDisposable
{
    private readonly ImagePort _imagePort = new("Image", PortDirection.Input, null!);
    private readonly ImagePort _grayscaleImagePort = new("Grayscale image", PortDirection.Output, null!);
    private bool disposedValue;

    public string DefaultName => "Image.Grayscale";

    public IEnumerable<IPort> Ports { get; }

    public ImageGrayscale()
    {
        Ports = new List<IPort>
        {
            _imagePort,
            _grayscaleImagePort
        };
    }

    public Task<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _grayscaleImagePort.Value?.Dispose();
        _grayscaleImagePort.Value = _imagePort.Value.ToGrayscale();
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
                _grayscaleImagePort?.Dispose();
            }
            disposedValue = true;
        }
    }
}
