using AyBorg.Plugins.ZXing.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.ImageProcessing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AyBorg.Plugins.ZXing.Tests
{
    public sealed class BarcodeReadTests
    {

        private readonly NullLogger<BarcodeRead> _logger = new(); 

        [Fact]
        public async Task Test_TryRunAsync_Success()
        {
            var readerBarcode = new BarcodeRead(_logger);
            var imagePort = readerBarcode.Ports.Single(x => x.Name == "Image")  as ImagePort;
            imagePort!.Value = Image.Load("./resources/Code-128.png");
            var formatPort = readerBarcode.Ports.Single(x => x.Name == "Barcode Format")  as EnumPort;
            formatPort!.Value = BarcodeFormats.CODE_128;


            bool result = await readerBarcode.TryRunAsync(CancellationToken.None);

            Assert.True(result);
            Assert.Equal("ABC-abc-1234", (readerBarcode.Ports.Single(x => x.Name == "String")  as StringPort)!.Value);
        }


        [Fact]
        public async Task Test_TryRunAsync_QrCodeReadAllFormats_Success()
        {
            var readerBarcode = new BarcodeRead(_logger);
            var imagePort = readerBarcode.Ports.Single(x => x.Name == "Image")  as ImagePort;
            imagePort!.Value = Image.Load("./resources/qr-code.png");
            var formatPort = readerBarcode.Ports.Single(x => x.Name == "Barcode Format")  as EnumPort;
            formatPort!.Value = BarcodeFormats.Undefined;


            bool result = await readerBarcode.TryRunAsync(CancellationToken.None);

            Assert.True(result);
            Assert.Equal("https://123TestTest567", (readerBarcode.Ports.Single(x => x.Name == "String")  as StringPort)!.Value);
        }

        [Fact]
        public async Task Test_TryRunAsync_QrCodeReadOnWrongFormat_Fail()
        {
            var readerBarcode = new BarcodeRead(_logger);
            var imagePort = readerBarcode.Ports.Single(x => x.Name == "Image")  as ImagePort;
            imagePort!.Value = Image.Load("./resources/qr-code.png");
            var formatPort = readerBarcode.Ports.Single(x => x.Name == "Barcode Format")  as EnumPort;
            formatPort!.Value = BarcodeFormats.CODE_128;


            bool result = await readerBarcode.TryRunAsync(CancellationToken.None);

            Assert.False(result);
            Assert.Equal(String.Empty, (readerBarcode.Ports.Single(x => x.Name == "String")  as StringPort)!.Value);
        }


        [Fact]
        public async Task Test_TryRunAsync_NoBarcode_Fail()
        {
            var readerBarcode = new BarcodeRead(_logger);
            var imagePort = readerBarcode.Ports.Single(x => x.Name == "Image")  as ImagePort;
            imagePort!.Value = Image.Load("./resources/Stickman.png");
            var formatPort = readerBarcode.Ports.Single(x => x.Name == "Barcode Format")  as EnumPort;
            formatPort!.Value = BarcodeFormats.CODE_128;


            bool result = await readerBarcode.TryRunAsync(CancellationToken.None);

            Assert.False(result);
            Assert.Equal(String.Empty, (readerBarcode.Ports.Single(x => x.Name == "String")  as StringPort)!.Value);
        }


    }
}