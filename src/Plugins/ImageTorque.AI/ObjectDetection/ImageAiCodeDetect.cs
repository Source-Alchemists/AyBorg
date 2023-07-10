using AyBorg.SDK.Common.Ports;
using ImageTorque.AI.Yolo;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.ImageTorque.AI;

public sealed class ImageAiCodeDetect : ImageAiDetectBase, IDisposable
{
    private readonly EnumPort _searchType = new("Type", PortDirection.Input, CodeType.All);
    private readonly YoloDetector<YoloV5CodeDetectorModel> _detector;
    private bool _disposedValue;

    public override string Name => "Image.AI.Code.Detect";

    public ImageAiCodeDetect(ILogger<ImageAiCodeDetect> logger) : base(logger)
    {
        Ports = new List<IPort>
        {
            _imagePort,
            _searchType,
            _thresholdPort,
            _regionsPort,
            _labelsPort,
            _scoredPort
        };

        _detector = new YoloDetector<YoloV5CodeDetectorModel>(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!, "./resources", "codeDetect.onnx"));
    }

    protected override bool SkipFilter(YoloPrediction prediction)
    {
        if (_searchType.Value.Equals(CodeType.Code1D) && !prediction.Label.Name.Equals("1d_code", StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }
        else if (_searchType.Value.Equals(CodeType.Code2D) && !prediction.Label.Name.Equals("2d_code", StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }
        return false;
    }

    protected override List<YoloPrediction> Predict() => _detector.Predict(_imagePort.Value);

    private void Dispose(bool disposing)
    {
        if (!_disposedValue && disposing)
        {
            _detector?.Dispose();
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public enum CodeType
    {
        All = 0,
        Code1D = 1,
        Code2D = 2
    }
}
