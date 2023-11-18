using System.Collections.Immutable;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using ImageTorque;
using ImageTorque.AI.Yolo;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.ImageTorque.AI;

public abstract class ImageAiDetectBase : IStepBody
{
    private readonly ILogger<ImageAiDetectBase> _logger;
    protected readonly ImagePort _imagePort = new("Image", PortDirection.Input);
    protected readonly NumericPort _thresholdPort = new("Threshold", PortDirection.Input, 0.6, 0.1, 1d);
    protected readonly RectangleCollectionPort _regionsPort = new("Regions", PortDirection.Output);
    protected readonly StringCollectionPort _labelsPort = new("Labels", PortDirection.Output);
    protected readonly NumericCollectionPort _scoredPort = new("Scores", PortDirection.Output);
    public abstract string Name { get; }
    public IReadOnlyCollection<string> Categories => new List<string> { DefaultStepCategories.ImageProcessing, DefaultStepCategories.Ai };
    public IReadOnlyCollection<IPort> Ports { get; protected set; } = null!;
    protected abstract bool SkipFilter(YoloPrediction prediction);
    protected abstract List<YoloPrediction> Predict();

    protected ImageAiDetectBase(ILogger<ImageAiDetectBase> logger)
    {
        _logger = logger;
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        try
        {
            List<YoloPrediction> predictions = Predict();
            var results = new List<Result>();
            var rectangles = new List<Rectangle>();

            foreach (YoloPrediction pred in predictions)
            {
                if (SkipFilter(pred))
                {
                    continue;
                }

                if (pred.Score < _thresholdPort.Value) continue;

                results.Add(new Result(pred.Rectangle, pred.Label.Name, pred.Score));

                rectangles.Add(pred.Rectangle);
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
}
