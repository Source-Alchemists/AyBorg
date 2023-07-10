using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Plugins.ImageTorque.Display;

public sealed class ImageObjectsDisplay : IStepBody
{
    private readonly ImagePort _imagePort = new("Image", PortDirection.Input);
    private readonly RectangleCollectionPort _regionsPort = new("Regions", PortDirection.Input);
    private readonly StringCollectionPort _labelsPort = new("Labels", PortDirection.Input);
    private readonly NumericCollectionPort _scoresPort = new("Scores", PortDirection.Input);
    public string Name => "Image.Objects.Display";

    public IReadOnlyCollection<string> Categories => new List<string> { DefaultStepCategories.Display, DefaultStepCategories.Ai };

    public IEnumerable<IPort> Ports { get; }

    public ImageObjectsDisplay()
    {
        Ports = new List<IPort>
        {
            _imagePort,
            _regionsPort,
            _labelsPort,
            _scoresPort
        };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken) => ValueTask.FromResult(true);
}
