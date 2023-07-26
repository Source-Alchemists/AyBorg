using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Communication;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.System.Runtime;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Plugins.Base.Communication.Tests;

public class CommunicationImageSendTests
{
    private static readonly NullLogger<CommunicationImageSend> s_nullLogger = new();
    private readonly Mock<IDeviceManager> _deviceManagerMock = new();
    private readonly Mock<ICommunicationDevice> _deviceMock = new();
    private readonly Mock<ICommunicationStateProvider> _communicationStateProviderMock = new();

    public CommunicationImageSendTests()
    {

        _deviceMock.Setup(m => m.Id).Returns("1");
        _deviceMock.Setup(m => m.Name).Returns("MockDevice");

        _deviceManagerMock.Setup(m => m.GetDevice<ICommunicationDevice>("1")).Returns(_deviceMock.Object);
    }

    [Theory]
    [InlineData(false, true, false, false, false)]
    [InlineData(true, false, false, false, false)]
    [InlineData(true, true, true, true, false)]
    [InlineData(true, true, true, true, true)]
    public async Task Test_TryRunAsync(bool expectedResult, bool sendAllowed, bool hasDevice, bool deviceCanSend, bool parallel)
    {
        // Arrange
        using var plugin = new CommunicationImageSend(s_nullLogger, _deviceManagerMock.Object, _communicationStateProviderMock.Object);
        _communicationStateProviderMock.Setup(m => m.IsResultCommunicationEnabled).Returns(sendAllowed);
        _deviceMock.Setup(m => m.TrySendAsync(It.IsAny<string>(), It.IsAny<IPort>())).ReturnsAsync(deviceCanSend);

        var parallelPort = (BooleanPort)plugin.Ports.First(p => p.Name.Equals("Parallel"));
        parallelPort.Value = parallel;

        if (hasDevice)
        {
            var devicePort = (SelectPort)plugin.Ports.First(p => p.Name.Equals("Device"));
            devicePort.Value = new SelectPort.ValueContainer("1", new List<string> { "1" });
            _deviceManagerMock.Setup(m => m.GetDevices<ICommunicationDevice>()).Returns(new List<ICommunicationDevice> { _deviceMock.Object });
        }

        // Act
        // Simulate lifecycle
        await plugin.AfterInitializedAsync();
        await plugin.BeforeStartAsync();
        bool result = await plugin.TryRunAsync(CancellationToken.None);
        if(parallel)
        {
            result = await plugin.TryRunAsync(CancellationToken.None);
        }

        // Assert
        Assert.Equal(expectedResult, result);
    }
}
