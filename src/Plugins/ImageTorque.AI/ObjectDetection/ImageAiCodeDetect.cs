using System.Collections.Immutable;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using ImageTorque;
using ImageTorque.AI.Yolo;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.ImageTorque.AI;

public sealed class ImageAiCodeDetect : ImageAiDetectBase, IDisposable
{
    private readonly ILogger<ImageAiCodeDetect> _logger;
    private readonly EnumPort _searchType = new("Type", PortDirection.Input, CodeType.All);
    private readonly YoloDetector<YoloV5CodeDetectorModel> _detector;
    private bool _disposedValue;

    public override string DefaultName => "Image.AI.Code.Detect";

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

        _detector = new YoloDetector<YoloV5CodeDetectorModel>(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!, "./resources", "codeDetect.onnx"));
    }

    public override ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        try
        {
            List<YoloPrediction> predictions = _detector.Predict(_imagePort.Value);
            var results = new List<Result>();
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

                results.Add(new Result(pred.Rectangle, pred.Label.Name, pred.Score));

                rectangles.Add(pred.Rectangle);
                labels.Add(pred.Label.Name);
                scores.Add(pred.Score);
            }

            results = results.OrderByDescending(r => r.Score).ToList();

            _regionsPort.Value = results.Select(r => r.Rectangle).ToImmutableList();
            _labelsPort.Value = results.Select(r => r.Label).ToImmutableList();
            _scoredPort.Value = results.Select(r => r.Score).ToImmutableList();

            return ValueTask.FromResult(true);
        }
        catch (NullReferenceException ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Plugin), ex, "{Message}", ex.Message);
        }

        return ValueTask.FromResult(false);
    }

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

    public enum CodeType
    {
        All = 0,
        Code1D = 1,
        Code2D = 2
    }
}
