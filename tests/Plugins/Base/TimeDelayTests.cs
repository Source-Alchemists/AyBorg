using AyBorg.SDK.Common.Ports;

namespace AyBorg.Plugins.Base.Tests;

public class TimeDelayTests
{
    private readonly TimeDelay _plugin;

    public TimeDelayTests()
    {
        _plugin = new TimeDelay();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async ValueTask Test_TryRunAsync(bool isCanceled)
    {
        // Arrange
        var numericPort = (NumericPort)_plugin.Ports.First(p => p.Name.Equals("Milliseconds"));
        numericPort.Value = 10;
        var tokenSource = new CancellationTokenSource();
        CancellationToken token = tokenSource.Token;
        if(isCanceled)
        {
            tokenSource.Cancel();
        }

        // Act
        bool result = await _plugin.TryRunAsync(token);

        // Assert
        Assert.Equal(!isCanceled, result);
    }
}
