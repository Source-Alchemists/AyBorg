using AyBorg.SDK.Common.Ports;
using ImageTorque;
using Microsoft.Extensions.Logging.Abstractions;

namespace AyBorg.Plugins.ImageTorque.AI;

public class ImageAiCodeDetectTests
{
    private static readonly NullLogger<ImageAiCodeDetect> s_logger = new();
    [Fact]
    public async Task Test_DetectPZNx3()
    {
        // Arrange
        using var plugin = new ImageAiCodeDetect(s_logger);
        var imagePort = (ImagePort)plugin.Ports.First(p => p.Name.Equals("Image"));
        var regionsPort = (RectangleCollectionPort)plugin.Ports.First(p => p.Name.Equals("Regions"));
        var labelsPort = (StringCollectionPort)plugin.Ports.First(p => p.Name.Equals("Labels"));
        var scoresPort = (NumericCollectionPort)plugin.Ports.First(p => p.Name.Equals("Scores"));
        using var image = Image.Load(Path.Combine("./resources", "pzn_3.jpeg"));
        imagePort.Value = image;

        // Act
        bool result = await plugin.TryRunAsync(CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(3, regionsPort.Value.Count);
        Assert.Equal(3, labelsPort.Value.Count);
        Assert.Equal(3, scoresPort.Value.Count);

        Assert.All(labelsPort.Value, v => v.Equals("1d_code"));
    }
}
