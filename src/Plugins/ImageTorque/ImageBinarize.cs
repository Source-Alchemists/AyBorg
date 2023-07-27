using System.Globalization;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using ImageTorque;

namespace AyBorg.Plugins.ImageTorque;

public sealed class ImageBinarize : IStepBody, IDisposable
{
    private readonly ImagePort _inputImagePort = new("Image", PortDirection.Input, null!);
    private readonly NumericPort _thresholdPort = new("Threshold", PortDirection.Input, 0.5d, 0d, 1d);
    private readonly ImagePort _outputImagePort = new("Binarized image", PortDirection.Output, null!);
    private readonly EnumPort _thresholdTypePort = new("Mode", PortDirection.Input, BinaryThresholdMode.Lumincance);
    private bool _disposedValue;

    public string Name => "Image.Binarize";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.ImageProcessing };

    public IReadOnlyCollection<IPort> Ports { get; }

    public ImageBinarize()
    {
        Ports = new IPort[]
        {
            _inputImagePort,
            _thresholdTypePort,
            _thresholdPort,
            _outputImagePort
        };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        float threshold = Convert.ToSingle(_thresholdPort.Value, CultureInfo.InvariantCulture);
        var mode = (BinaryThresholdMode)_thresholdTypePort.Value;
        _outputImagePort.Value?.Dispose();
        _outputImagePort.Value = _inputImagePort.Value.Binarize(threshold, mode);

        return ValueTask.FromResult(true);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _outputImagePort.Dispose();
            }
            _disposedValue = true;
        }
    }
}
