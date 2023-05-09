using AyBorg.SDK.Common.Ports;
using ImageTorque.AI.Yolo;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.ImageTorque.AI;

public class ImageAiVialDetect : ImageAiDetectBase, IDisposable
{
    private readonly EnumPort _searchType = new("Type", PortDirection.Input, ObjectType.All);
    private readonly YoloDetector<YoloV5VialDetectorModel> _detector;
    private bool _disposedValue;

    public override string DefaultName => "Image.AI.Vial.Detect";

    public ImageAiVialDetect(ILogger<ImageAiVialDetect> logger) : base(logger)
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

        _detector = new YoloDetector<YoloV5VialDetectorModel>(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!, "./resources", "vialDetect.onnx"));
    }

    protected override bool SkipFilter(YoloPrediction prediction)
    {
        if (_searchType.Value.Equals(ObjectType.Vial) && !prediction.Label.Name.Equals("Vial", StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }
        else if (_searchType.Value.Equals(ObjectType.Cap) && !prediction.Label.Name.Equals("Cap", StringComparison.InvariantCultureIgnoreCase))
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
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public enum ObjectType
    {
        All = 0,
        Vial = 1,
        Cap = 2
    }
}
