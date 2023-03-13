using AyBorg.Plugins.ZXing.Models;
using AyBorg.SDK.Common.Ports;
using ImageTorque;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AyBorg.Plugins.ZXing.Tests
{
    public sealed class ImageCodeReadTests
    {
        private readonly NullLogger<ImageCodeRead> _logger = new();

        [Fact]
        public async Task Test_TryRunAsync_Success()
        {
            // Arrange
            var readerBarcode = new ImageCodeRead(_logger);
            var imagePort = readerBarcode.Ports.Single(x => x.Name == "Image") as ImagePort;
            imagePort!.Value = Image.Load("./resources/Code-128.png");
            var formatPort = readerBarcode.Ports.Single(x => x.Name == "Code format") as EnumPort;
            formatPort!.Value = CodeFormats.CODE_128;

            // Act
            bool result = await readerBarcode.TryRunAsync(CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal("ABC-abc-1234", (readerBarcode.Ports.Single(x => x.Name == "Codes") as StringCollectionPort)!.Value.First());
        }

        [Fact]
        public async Task Test_TryRunAsync_ReadAllFormats_Success()
        {
            // Arrange
            var readerBarcode = new ImageCodeRead(_logger);
            var imagePort = readerBarcode.Ports.Single(x => x.Name == "Image") as ImagePort;
            imagePort!.Value = Image.Load("./resources/Code-128.png");
            var formatPort = readerBarcode.Ports.Single(x => x.Name == "Code format") as EnumPort;
            formatPort!.Value = CodeFormats.All;

            // Act
            bool result = await readerBarcode.TryRunAsync(CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal("ABC-abc-1234", (readerBarcode.Ports.Single(x => x.Name == "Codes") as StringCollectionPort)!.Value.First());
        }

        [Fact]
        public async Task Test_TryRunAsync_ReadAllBarcodeFormats_Success()
        {
            // Arrange
            var readerBarcode = new ImageCodeRead(_logger);
            var imagePort = readerBarcode.Ports.Single(x => x.Name == "Image") as ImagePort;
            imagePort!.Value = Image.Load("./resources/Code-128.png");
            var formatPort = readerBarcode.Ports.Single(x => x.Name == "Code format") as EnumPort;
            formatPort!.Value = CodeFormats.All_Barcodes;

            // Act
            bool result = await readerBarcode.TryRunAsync(CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal("ABC-abc-1234", (readerBarcode.Ports.Single(x => x.Name == "Codes") as StringCollectionPort)!.Value.First());
        }

        [Fact]
        public async Task Test_TryRunAsync_ReadAllMatrixBarcodeFormats_Success()
        {
            // Arrange
            var readerBarcode = new ImageCodeRead(_logger);
            var imagePort = readerBarcode.Ports.Single(x => x.Name == "Image") as ImagePort;
            imagePort!.Value = Image.Load("./resources/qr-code.png");
            var formatPort = readerBarcode.Ports.Single(x => x.Name == "Code format") as EnumPort;
            formatPort!.Value = CodeFormats.All_MatrixBarcodes;

            // Act
            bool result = await readerBarcode.TryRunAsync(CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal("https://123TestTest567", (readerBarcode.Ports.Single(x => x.Name == "Codes") as StringCollectionPort)!.Value.First());
        }

        [Fact]
        public async Task Test_TryRunAsync_WrongFormat_Fail()
        {
            // Arrange
            var readerBarcode = new ImageCodeRead(_logger);
            var imagePort = readerBarcode.Ports.Single(x => x.Name == "Image") as ImagePort;
            imagePort!.Value = Image.Load("./resources/qr-code.png");
            var formatPort = readerBarcode.Ports.Single(x => x.Name == "Code format") as EnumPort;
            formatPort!.Value = CodeFormats.CODE_128;

            // Act
            bool result = await readerBarcode.TryRunAsync(CancellationToken.None);

            // Assert
            Assert.False(result);
            Assert.Empty((readerBarcode.Ports.Single(x => x.Name == "Codes") as StringCollectionPort)!.Value);
        }

        [Fact]
        public async Task Test_TryRunAsync_NoBarcode_Fail()
        {
            // Arrange
            var readerBarcode = new ImageCodeRead(_logger);
            var imagePort = readerBarcode.Ports.Single(x => x.Name == "Image") as ImagePort;
            imagePort!.Value = Image.Load("./resources/Stickman.png");
            var formatPort = readerBarcode.Ports.Single(x => x.Name == "Code format") as EnumPort;
            formatPort!.Value = CodeFormats.CODE_128;

            // Act
            bool result = await readerBarcode.TryRunAsync(CancellationToken.None);

            // Assert
            Assert.False(result);
            Assert.Empty((readerBarcode.Ports.Single(x => x.Name == "Codes") as StringCollectionPort)!.Value);
        }
    }
}
