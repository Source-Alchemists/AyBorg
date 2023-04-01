using System.Collections.ObjectModel;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using ImageTorque;
using ImageTorque.AI.Yolo;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.ImageTorque.AI;

public class ImageAiCodeDetect : IStepBody, IDisposable
{
    private readonly ILogger<ImageAiCodeDetect> _logger;
    private readonly ImagePort _imagePort = new("Image", PortDirection.Input);
    private readonly EnumPort _searchType = new("Type", PortDirection.Input, CodeType.All);
    private readonly NumericPort _thresholdPort = new("Threshold", PortDirection.Input, 0.6, 0.1, 1d);
    private readonly RectangleCollectionPort _regionsPort = new("Regions", PortDirection.Output, new ReadOnlyCollection<Rectangle>(Array.Empty<Rectangle>()));
    private readonly StringCollectionPort _labelsPort = new("Labels", PortDirection.Output, new ReadOnlyCollection<string>(Array.Empty<string>()));
    private readonly NumericCollectionPort _scoredPort = new("Scores", PortDirection.Output, new ReadOnlyCollection<double>(Array.Empty<double>()));
    private readonly YoloDetector<YoloV5CodeDetectorModel> _detector;
    private bool _disposedValue;

    public string DefaultName => "Image.AI.Code.Detect";

    public IReadOnlyCollection<string> Categories => new List<string> { DefaultStepCategories.ImageProcessing, DefaultStepCategories.Ai };

    public IEnumerable<IPort> Ports { get; }

    public ImageAiCodeDetect(ILogger<ImageAiCodeDetect> logger)
    {
        _logger = logger;

        Ports = new List<IPort>
        {
            _imagePort,
            _searchType,
            _thresholdPort,
            _regionsPort,
            _labelsPort,
            _scoredPort
        };

        _detector = new YoloDetector<YoloV5CodeDetectorModel>(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!, "./resources", "codeDetector.onnx"));
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        try
        {
            List<YoloPrediction> predictions = _detector.Predict(_imagePort.Value);
            var rectangles = new List<Rectangle>();
            var labels = new List<string>();
            var scores = new List<double>();

            foreach (YoloPrediction pred in predictions)
            {
                if (_searchType.Value.Equals(CodeType.Code1D) && !pred.Label.Name.Equals("1d_code", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                else if (_searchType.Value.Equals(CodeType.Code2D) && !pred.Label.Name.Equals("2d_code", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (pred.Score < _thresholdPort.Value) continue;

                rectangles.Add(pred.Rectangle);
                labels.Add(pred.Label.Name);
                scores.Add(pred.Score);
            }

            _regionsPort.Value = new ReadOnlyCollection<Rectangle>(rectangles);
            _labelsPort.Value = new ReadOnlyCollection<string>(labels);
            _scoredPort.Value = new ReadOnlyCollection<double>(scores);

            return ValueTask.FromResult(true);
        }
        catch (NullReferenceException ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Plugin), ex, "{Message}", ex.Message);

        }
        catch (DirectoryNotFoundException ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Plugin), ex, "{Message}", ex.Message);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Plugin), ex, "{Message}", ex.Message);
        }

        return ValueTask.FromResult(false);
    }

    protected virtual void Dispose(bool disposing)
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

    public enum CodeType
    {
        All = 0,
        Code1D = 1,
        Code2D = 2
    }
}
