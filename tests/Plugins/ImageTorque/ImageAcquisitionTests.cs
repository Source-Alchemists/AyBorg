using AyBorg.SDK.Common;
using AyBorg.SDK.Common.ImageAcquisition;
using AyBorg.SDK.Common.Ports;
using ImageTorque;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Plugins.ImageTorque.Tests;

public class ImageAcquisitionTests : IDisposable
{
    private static readonly NullLogger<ImageAcquisition> s_nullLogger = new();
    private readonly Mock<IDeviceManager> _deviceManagerMock = new();
    private readonly Mock<ICameraDevice> _deviceMock = new();
    private readonly Image _image;
    private readonly ImageAcquisition _plugin;
    private bool _isDisposed = false;

    public ImageAcquisitionTests()
    {
        _image = Image.Load("./resources/luna.jpg");
        _deviceMock.Setup(m => m.Id).Returns("123");

        _deviceManagerMock.Setup(m => m.GetDevices<ICameraDevice>()).Returns(new List<ICameraDevice> {
            _deviceMock.Object
        });
        _plugin = new ImageAcquisition(s_nullLogger, _deviceManagerMock.Object);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task Test_AfterInitializedAsync(bool hasDevices, bool devicePortSet)
    {
        // Arrange
        var devicePort = (SelectPort)_plugin.Ports.First(p => p.Name.Equals("Device"));
        devicePort.Value = devicePortSet ? new SelectPort.ValueContainer("123", new List<string> { "123" }) : null!;

        if (!hasDevices)
        {
            _deviceManagerMock.Setup(m => m.GetDevices<ICameraDevice>()).Returns(new List<ICameraDevice>());
        }

        // Act
        await _plugin.AfterInitializedAsync();

        // Assert
        if (devicePortSet)
        {
            Assert.Equal("123", devicePort.Value.SelectedValue);
            _deviceManagerMock.Verify(m => m.GetDevice<ICameraDevice>(devicePort.Value.SelectedValue), Times.Once);
        }
        else
        {
            Assert.Equal(string.Empty, devicePort.Value.SelectedValue);
            _deviceManagerMock.Verify(m => m.GetDevice<ICameraDevice>(It.IsAny<string>()), Times.Never);
        }
    }

    [Fact]
    public async Task Test_BeforeStartAsync()
    {
        // Arrange
        var devicePort = (SelectPort)_plugin.Ports.First(p => p.Name.Equals("Device"));
        devicePort.Value = new SelectPort.ValueContainer("123", new List<string> { "123" });

        // Act
        await _plugin.BeforeStartAsync();

        // Assert
        Assert.Equal("123", devicePort.Value.SelectedValue);
        _deviceManagerMock.Verify(m => m.GetDevice<ICameraDevice>(devicePort.Value.SelectedValue), Times.Once);
    }

    [Fact]
    public void Test_OnDeviceCollectionChanged()
    {
        // Arrange
        var devicePort = (SelectPort)_plugin.Ports.First(p => p.Name.Equals("Device"));
        devicePort.Value = new SelectPort.ValueContainer("123", new List<string> { "123" });

        // Act
        _deviceManagerMock.Raise(m => m.DeviceCollectionChanged += null, new CollectionChangedEventArgs(Array.Empty<object>(), Array.Empty<object>(), Array.Empty<object>()));

        // Assert
        Assert.Equal("123", devicePort.Value.SelectedValue);
        _deviceManagerMock.Verify(m => m.GetDevice<ICameraDevice>(devicePort.Value.SelectedValue), Times.Once);
    }

    [Theory]
    [InlineData(true, true, true, true)]
    [InlineData(false, true, false, true)]
    [InlineData(false, true, true, false)]
    public async Task Test_TryRunAsync(bool expectedResult, bool hasDevice, bool hasImageContainer, bool hasImage)
    {
        // Arrange
        var devicePort = (SelectPort)_plugin.Ports.First(p => p.Name.Equals("Device"));
        devicePort.Value = new SelectPort.ValueContainer("123", new List<string> { "123" });

        var imagePort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Image"));

        if (hasDevice)
        {
            _deviceManagerMock.Setup(m => m.GetDevice<ICameraDevice>("123")).Returns(_deviceMock.Object);
        }

        if (hasImageContainer)
        {
            _deviceMock.Setup(m => m.AcquisitionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new ImageContainer(hasImage ? _image : null!, 1, "TestInfo"));
        }

        // Act
        bool result = await _plugin.TryRunAsync(CancellationToken.None);

        // Assert
        Assert.Equal(expectedResult, result);
        if (expectedResult)
        {
            Assert.NotNull(imagePort.Value);
        }
        else
        {
            Assert.Null(imagePort.Value);
        }
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
            _image?.Dispose();
            _isDisposed = true;
        }
    }
}
