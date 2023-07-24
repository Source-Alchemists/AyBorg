using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Communication;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.System.Runtime;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Plugins.Base.Communication.Tests;

public class CommunicationSendTests : IDisposable
{
    private static readonly NullLogger<CommunicationSend> s_nullLogger = new();
    private readonly Mock<IDeviceManager> _deviceManagerMock = new();
    private readonly Mock<ICommunicationDevice> _deviceMock = new();
    private readonly Mock<ICommunicationStateProvider> _communicationStateProviderMock = new();
    private readonly CommunicationSend _plugin;
    private bool _isDisposed = false;

    public CommunicationSendTests()
    {

        _deviceMock.Setup(m => m.Id).Returns("1");
        _deviceMock.Setup(m => m.Name).Returns("MockDevice");

        _deviceManagerMock.Setup(m => m.GetDevice<ICommunicationDevice>("1")).Returns(_deviceMock.Object);

        _plugin = new CommunicationSend(s_nullLogger, _deviceManagerMock.Object, _communicationStateProviderMock.Object);
    }

    [Theory]
    [InlineData(false, true, false, false, false)]
    [InlineData(true, false, false, false, false)]
    [InlineData(true, true, true, true, false)]
    [InlineData(true, true, true, true, true)]
    public async Task Test_TryRunAsync(bool expectedResult, bool sendAllowed, bool hasDevice, bool deviceCanSend, bool parallel)
    {
        // Arrange
        _communicationStateProviderMock.Setup(m => m.IsResultCommunicationEnabled).Returns(sendAllowed);
        _deviceMock.Setup(m => m.TrySendAsync(It.IsAny<string>(), It.IsAny<IPort>())).ReturnsAsync(deviceCanSend);

        var parallelPort = (BooleanPort)_plugin.Ports.First(p => p.Name.Equals("Parallel"));
        parallelPort.Value = parallel;

        if (hasDevice)
        {
            var devicePort = (SelectPort)_plugin.Ports.First(p => p.Name.Equals("Device"));
            devicePort.Value = new SelectPort.ValueContainer("1", new List<string> { "1" });
            _deviceManagerMock.Setup(m => m.GetDevices<ICommunicationDevice>()).Returns(new List<ICommunicationDevice> { _deviceMock.Object });
        }

        // Act
        // Simulate lifecycle
        await _plugin.AfterInitializedAsync();
        await _plugin.BeforeStartAsync();
        bool result = await _plugin.TryRunAsync(CancellationToken.None);
        if(parallel)
        {
            result = await _plugin.TryRunAsync(CancellationToken.None);
        }

        // Assert
        Assert.Equal(expectedResult, result);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
        if (isDisposing && !_isDisposed)
        {
            _plugin?.Dispose();
            _isDisposed = true;
        }
    }
}
