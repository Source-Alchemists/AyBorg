using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using ImageTorque;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.ImageTorque;

public sealed class ImageArithemtic : IStepBody, IDisposable
{
    private readonly ILogger<ImageArithemtic> _logger;
    private readonly ImagePort _inputImageAPort = new("Image A", PortDirection.Input, null!);
    private readonly ImagePort _inputImageBPort = new("Image B", PortDirection.Input, null!);
    private readonly ImagePort _resultImagePort = new("Image", PortDirection.Output, null!);
    private readonly EnumPort _opertionPort = new("Operation", PortDirection.Input, ImageMathMode.Add);
    private bool _disposedValue;

    public string Name => "Image.Arithmetic";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.ImageProcessing, DefaultStepCategories.Math };

    public IReadOnlyCollection<IPort> Ports { get; }

    public ImageArithemtic(ILogger<ImageArithemtic> logger)
    {
        _logger = logger;

        Ports = new IPort[]
        {
            _inputImageAPort,
            _inputImageBPort,
            _opertionPort,
            _resultImagePort
        };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _resultImagePort.Value?.Dispose();
        try
        {
            Image imageA = _inputImageAPort.Value;
            Image imageB = _inputImageBPort.Value;
            ImageMathMode operationMode = (ImageMathMode)_opertionPort.Value;

            switch (operationMode)
            {
                case ImageMathMode.Add:
                    _resultImagePort.Value = imageA.Add(imageB);
                    break;
                case ImageMathMode.Subtract:
                    _resultImagePort.Value = imageA.Subtract(imageB);
                    break;
                case ImageMathMode.Multiply:
                    _resultImagePort.Value = imageA.Multiply(imageB);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Result), ex, "Failed to perform image arithmetic operation");
            return ValueTask.FromResult(false);
        }

        return ValueTask.FromResult(true);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue && disposing)
        {
            _resultImagePort?.Dispose();
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
