using AyBorg.Plugins.ZXing.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.ImageProcessing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AyBorg.Plugins.ZXing.Tests
{
    public sealed class BarcodeMatrixReadTests
    {

        private readonly NullLogger<BarcodeMatrixRead> _logger = new(); 

        [Fact]
        public async Task Test_TryRunAsync_Success()
        {
            var readerMatrixBarcode = new BarcodeMatrixRead(_logger);
            var imagePort = readerMatrixBarcode.Ports.Single(x => x.Name == "Image")  as ImagePort;
            imagePort!.Value = Image.Load("./resources/qr-code.png");
            var formatPort = readerMatrixBarcode.Ports.Single(x => x.Name == "Matrix Barcode Format")  as EnumPort;
            formatPort!.Value = MatrixBarcodeFormats.QR_CODE;


            bool result = await readerMatrixBarcode.TryRunAsync(CancellationToken.None);

            Assert.True(result);
            Assert.Equal("https://123TestTest567", (readerMatrixBarcode.Ports.Single(x => x.Name == "String")  as StringPort)!.Value);
        }


        [Fact]
        public async Task Test_TryRunAsync_ReadAllFormats_Success()
        {
            var readerMatrixBarcode = new BarcodeMatrixRead(_logger);
            var imagePort = readerMatrixBarcode.Ports.Single(x => x.Name == "Image")  as ImagePort;
            imagePort!.Value = Image.Load("./resources/qr-code.png");
            var formatPort = readerMatrixBarcode.Ports.Single(x => x.Name == "Matrix Barcode Format")  as EnumPort;
            formatPort!.Value = MatrixBarcodeFormats.Undefined;


            bool result = await readerMatrixBarcode.TryRunAsync(CancellationToken.None);

            Assert.True(result);
            Assert.Equal("https://123TestTest567", (readerMatrixBarcode.Ports.Single(x => x.Name == "String")  as StringPort)!.Value);
        }

        [Fact]
        public async Task Test_TryRunAsync_MatrixCodeReadOnWrongFormat_Fail()
        {
            var readerMatrixBarcode = new BarcodeMatrixRead(_logger);
            var imagePort = readerMatrixBarcode.Ports.Single(x => x.Name == "Image")  as ImagePort;
            imagePort!.Value = Image.Load("./resources/qr-code.png");
            var formatPort = readerMatrixBarcode.Ports.Single(x => x.Name == "Matrix Barcode Format")  as EnumPort;
            formatPort!.Value = MatrixBarcodeFormats.MAXICODE;


            bool result = await readerMatrixBarcode.TryRunAsync(CancellationToken.None);

            Assert.False(result);
            Assert.Equal(String.Empty, (readerMatrixBarcode.Ports.Single(x => x.Name == "String")  as StringPort)!.Value);
        }


        [Fact]
        public async Task Test_TryRunAsync_NoMatrixBarcode_Fail()
        {
            var readerMatrixBarcode = new BarcodeMatrixRead(_logger);
            var imagePort = readerMatrixBarcode.Ports.Single(x => x.Name == "Image")  as ImagePort;
            imagePort!.Value = Image.Load("./resources/Code-128.png");
            var formatPort = readerMatrixBarcode.Ports.Single(x => x.Name == "Matrix Barcode Format")  as EnumPort;
            formatPort!.Value = MatrixBarcodeFormats.Undefined;


            bool result = await readerMatrixBarcode.TryRunAsync(CancellationToken.None);

            Assert.False(result);
            Assert.Equal(String.Empty, (readerMatrixBarcode.Ports.Single(x => x.Name == "String")  as StringPort)!.Value);
        }


    }
}