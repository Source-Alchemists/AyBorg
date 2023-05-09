using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Plugins.ImageTorque.AI;

public abstract class ImageAiDetectBase : IStepBody
{
    protected readonly ImagePort _imagePort = new("Image", PortDirection.Input);
    protected readonly NumericPort _thresholdPort = new("Threshold", PortDirection.Input, 0.6, 0.1, 1d);
    protected readonly RectangleCollectionPort _regionsPort = new("Regions", PortDirection.Output);
    protected readonly StringCollectionPort _labelsPort = new("Labels", PortDirection.Output);
    protected readonly NumericCollectionPort _scoredPort = new("Scores", PortDirection.Output);
    public abstract string DefaultName { get; }
    public IReadOnlyCollection<string> Categories => new List<string> { DefaultStepCategories.ImageProcessing, DefaultStepCategories.Ai };
    public IEnumerable<IPort> Ports { get; protected set; } = null!;
    public abstract ValueTask<bool> TryRunAsync(CancellationToken cancellationToken);
}
