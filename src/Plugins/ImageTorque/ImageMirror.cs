using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using ImageTorque;

namespace AyBorg.Plugins.ImageTorque;

public sealed class ImageMirror : IStepBody, IDisposable
{
    private readonly BooleanPort _mirrorVertical = new("Vertical", PortDirection.Input, false);
    private readonly BooleanPort _mirrorHorizontal = new("Horizontal", PortDirection.Input, false);
    private readonly ImagePort _inputImage = new("Image", PortDirection.Input, null!);
    private readonly ImagePort _outputImage = new("Mirrored image", PortDirection.Output, null!);
    private bool _disposedValue;

    public string Name => "Image.Mirror";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.ImageProcessing };

    public IReadOnlyCollection<IPort> Ports { get; }

    public ImageMirror()
    {
        Ports = new IPort[]
        {
            _inputImage,
            _mirrorHorizontal,
            _mirrorVertical,
            _outputImage
        };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _outputImage.Value?.Dispose();

        if (_mirrorHorizontal.Value == false && _mirrorVertical.Value == false)
        {
            // No mirror, just copy the input to the output
            _outputImage.Value = new Image(_inputImage.Value);
            return ValueTask.FromResult(true);
        }

        MirrorMode mirrorMode = MirrorMode.Horizontal;
        if (_mirrorHorizontal.Value && _mirrorVertical.Value)
        {
            mirrorMode = MirrorMode.VerticalHorizontal;
        }
        else if (_mirrorVertical.Value)
        {
            mirrorMode = MirrorMode.Vertical;
        }

        _outputImage.Value = new Image(_inputImage.Value.Mirror(mirrorMode));

        return ValueTask.FromResult(true);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _outputImage.Dispose();
            }
            _disposedValue = true;
        }
    }
}
