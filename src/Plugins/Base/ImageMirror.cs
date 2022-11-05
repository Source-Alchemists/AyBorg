using Atomy.SDK.ImageProcessing;
using Atomy.SDK.Common.Ports;
using Atomy.SDK.Common;

namespace Atomy.Plugins.Base;

public sealed class ImageMirror : IStepBody, IDisposable
{
    private readonly BooleanPort _mirrorVertical = new("Vertical", PortDirection.Input, false);
    private readonly BooleanPort _mirrorHorizontal = new("Horizontal", PortDirection.Input, false);
    private readonly ImagePort _inputImage = new("Image", PortDirection.Input, null!);
    private readonly ImagePort _outputImage = new("Mirrored image", PortDirection.Output, null!);
    private bool disposedValue;

    public string DefaultName => "Image.Mirror";

    public IEnumerable<IPort> Ports { get; }

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

    public Task<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _outputImage.Value?.Dispose();
        
        if (_mirrorHorizontal.Value == false && _mirrorVertical.Value == false)
        {
            // No mirror, just copy the input to the output
            _outputImage.Value = new Image(_inputImage.Value);
            return Task.FromResult(true);
        }

        var mirrorMode = MirrorMode.Horizontal;
        if (_mirrorHorizontal.Value && _mirrorVertical.Value)
        {
            mirrorMode = MirrorMode.VerticalHorizontal;
        }
        else if (_mirrorVertical.Value)
        {
            mirrorMode = MirrorMode.Vertical;
        }

        _outputImage.Value = new Image(_inputImage.Value.Mirror(mirrorMode));

        return Task.FromResult(true);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _inputImage.Dispose();
                _outputImage.Dispose();
            }
            disposedValue = true;
        }
    }
}