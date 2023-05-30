using AyBorg.SDK.Common.Ports;
using ImageTorque;
using Microsoft.Extensions.Logging.Abstractions;

namespace AyBorg.Plugins.ImageTorque.AI;

public class ImageAiVialDetectTests
{
    private static readonly NullLogger<ImageAiVialDetect> s_logger = new();

    [Fact]
    public async Task Test_Null_Image()
    {
        // Arrange
        using var plugin = new ImageAiVialDetect(s_logger);
        var imagePort = (ImagePort)plugin.Ports.First(p => p.Name.Equals("Image"));

        // Act
        bool result = await plugin.TryRunAsync(CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Test_DetectVialAndCapx1()
    {
        // Arrange
        using var plugin = new ImageAiVialDetect(s_logger);
        var imagePort = (ImagePort)plugin.Ports.First(p => p.Name.Equals("Image"));
        var regionsPort = (RectangleCollectionPort)plugin.Ports.First(p => p.Name.Equals("Regions"));
        var labelsPort = (StringCollectionPort)plugin.Ports.First(p => p.Name.Equals("Labels"));
        var scoresPort = (NumericCollectionPort)plugin.Ports.First(p => p.Name.Equals("Scores"));
        using var image = Image.Load(Path.Combine("./resources", "vial_1.jpeg"));
        imagePort.Value = image;

        // Act
        bool result = await plugin.TryRunAsync(CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(2, regionsPort.Value.Count);
        Assert.Equal(2, labelsPort.Value.Count);
        Assert.Equal(2, scoresPort.Value.Count);
        Assert.Single(labelsPort.Value.Where(p => p.Equals("Cap")));
        Assert.Single(labelsPort.Value.Where(p => p.Equals("Vial")));
    }

    [Fact]
    public async Task Test_DetectNothing()
    {
        // Arrange
        using var plugin = new ImageAiVialDetect(s_logger);
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
        Assert.Empty(regionsPort.Value);
        Assert.Empty(labelsPort.Value);
        Assert.Empty(scoresPort.Value);
    }
}
