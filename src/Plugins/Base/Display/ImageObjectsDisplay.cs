using System.Collections.ObjectModel;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using ImageTorque;

namespace AyBorg.Plugins.Base.Display;

public sealed class ImageObjectsDisplay : IStepBody
{
    private readonly ImagePort _imagePort = new("Image", PortDirection.Input);
    private readonly RectangleCollectionPort _regionsPort = new("Regions", PortDirection.Input, new ReadOnlyCollection<Rectangle>(Array.Empty<Rectangle>()));
    private readonly StringCollectionPort _labelsPort = new("Labels", PortDirection.Input, new ReadOnlyCollection<string>(Array.Empty<string>()));
    private readonly NumericCollectionPort _scoresPort = new("Scores", PortDirection.Input, new ReadOnlyCollection<double>(Array.Empty<double>()));
    public string DefaultName => "Image.Objects.Display";

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
