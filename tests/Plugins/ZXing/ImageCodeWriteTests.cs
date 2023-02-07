using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AyBorg.SDK.Common.Ports;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using ZXing;

namespace AyBorg.Plugins.ZXing.Tests
{
    public class ImageCodeWriteTests
    {

        private readonly NullLogger<ImageCodeWrite> _logger = new();
        private readonly NullLogger<ImageCodeRead> _loggerRead = new();

        [Fact]
        public async Task QRCode_WriteRead_Successful(){
        var writer = new ImageCodeWrite(_logger);
        var codePort = writer.Ports.Single(x => x.Name == "Code") as StringPort;
        codePort!.Value = "ABC-abc-1234";
        var formatPort = writer.Ports.Single(x => x.Name == "Code format") as EnumPort;
        formatPort!.Value = BarcodeFormat.QR_CODE;
        var widthPort = writer.Ports.Single(x => x.Name == "Width") as NumericPort;
        widthPort!.Value = 300;
        var heightPort = writer.Ports.Single(x => x.Name == "Height") as NumericPort;
        heightPort!.Value = 300;

        bool resultWrite = await writer.TryRunAsync(CancellationToken.None);

        var reader = new ImageCodeRead(_loggerRead);
        var imagePortWriter = writer.Ports.Single(x => x.Name == "Image") as ImagePort;
        var imagePortReader = reader.Ports.Single(x => x.Name == "Image") as ImagePort;
        imagePortReader!.Value = imagePortWriter!.Value;
        var formatPortReader = reader.Ports.Single(x => x.Name == "Code format") as EnumPort;
        formatPortReader!.Value = BarcodeFormat.QR_CODE;

        bool resultRead = await reader.TryRunAsync(CancellationToken.None);

        Assert.True(resultWrite);
        Assert.True(resultRead);
        Assert.Equal("ABC-abc-1234", (reader.Ports.Single(x => x.Name == "Code") as StringPort)!.Value);
        }
        
    }
}