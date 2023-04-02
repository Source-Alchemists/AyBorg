namespace AyBorg.Plugins.ImageTorque.Display.Tests;

public class ImageObjectsDisplayTests
{
    [Fact]
    public async Task Test_TryRunAsync()
    {
        // Arrange
        var plugin = new ImageObjectsDisplay();

        // Act
        bool result = await plugin.TryRunAsync(CancellationToken.None);

        // Assert
        Assert.True(result);
    }
}
