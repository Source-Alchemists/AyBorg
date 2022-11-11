using System.Globalization;
using Autodroid.SDK.ImageProcessing;
using Autodroid.SDK.Common.Ports;
using Autodroid.SDK.Common;

namespace Autodroid.Plugins.Base;

public sealed class ImageBinarize : IStepBody, IDisposable
{
    private readonly ImagePort _inputImagePort = new("Image", PortDirection.Input, null!);
    private readonly NumericPort _thresholdPort = new("Threshold", PortDirection.Input, 0.5d, 0d, 1d);
    private readonly ImagePort _outputImagePort = new("Binarized image", PortDirection.Output, null!);
    private readonly EnumPort _thresholdTypePort = new("Mode", PortDirection.Input, BinaryThresholdMode.Lumincance);
    private bool disposedValue;

    public string DefaultName => "Image.Binarize";

    public IEnumerable<IPort> Ports { get; }

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
        var threshold = System.Convert.ToSingle(_thresholdPort.Value, CultureInfo.InvariantCulture);
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
        if (!disposedValue)
        {
            if (disposing)
            {
                _outputImagePort.Dispose();
            }
            disposedValue = true;
        }
    }
}