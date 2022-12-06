using AyBorg.Plugins.OpenCvSharp;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.ImageProcessing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AyBorg.Plugins.OpenCvSharp.Tests
{
    public class QrCodeReadTests
    {
        private readonly NullLogger<QrCodeRead> _logger = new(); 

        [Theory]
        [InlineData("./resources/micro-qr-code.png", "ABC-abc-1234")]
        [InlineData("./resources/qr-code.png", "ABC-abc-1234")]
        // todo add more types
        public async Task Test_TryRunAsync_Success(string resourcePath, string expectedValue)
        {
             var readerBarcode = new QrCodeRead(_logger);
            var imagePort = readerBarcode.Ports.Single(x => x.Name == "Image")  as ImagePort;
            imagePort!.Value = Image.Load(resourcePath);


            bool result = await readerBarcode.TryRunAsync(CancellationToken.None);

            Assert.True(result);
            Assert.Equal(expectedValue, (readerBarcode.Ports.Single(x => x.Name == "String")  as StringPort)!.Value);

        }

        [Fact]
        public async Task Test_TryRunAsync_Fail()
        {
            var readerBarcode = new QrCodeRead(_logger);
            var imagePort = readerBarcode.Ports.Single(x => x.Name == "Image")  as ImagePort;
            imagePort!.Value = Image.Load("./resources/Stickman.png");


            bool result = await readerBarcode.TryRunAsync(CancellationToken.None);

            Assert.False(result);
            Assert.Equal(String.Empty, (readerBarcode.Ports.Single(x => x.Name == "String")  as StringPort)!.Value);


        }
    }
}