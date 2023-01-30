using System.Net.Mime;
using System.Runtime.CompilerServices;
using AyBorg.Plugins.ZXing.Models;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using ImageTorque;
using ImageTorque.Buffers;
using ImageTorque.Pixels;
using Microsoft.Extensions.Logging;
using ZXing;
using ZXing.Common;

namespace ZXing
{
    public class ImageCodeWrite : IStepBody
    {
        private readonly ILogger<ImageCodeWrite> _logger;
        private readonly EnumPort _formatPort = new("Code format", PortDirection.Input, BarcodeFormat.QR_CODE);
        private readonly StringPort _codePort = new("Code", PortDirection.Input, string.Empty);
        private readonly NumericPort _widthPort = new("Width", PortDirection.Input, 100);
        private readonly NumericPort _heightPort = new("Height", PortDirection.Input, 100);
        private readonly ImagePort _imagePort = new("Image", PortDirection.Output, null!);
        private byte[] _tmpBuffer = null!;
        public string DefaultName => "Image.Code.Write";

        public IEnumerable<string> Categories { get; } = new List<string> { DefaultStepCategories.ImageProcessing };

        public ImageCodeWrite(ILogger<ImageCodeWrite> logger)
        {
            _logger = logger;
            Ports = new IPort[]
            {
                _imagePort,
                _formatPort,
                _codePort,
            };
        }
        public IEnumerable<IPort> Ports { get; }

        public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken) {
            var writer = new BarcodeWriterGeneric{
                Options = new EncodingOptions{
                    Width = (int)_widthPort.Value,
                    Height = (int)_heightPort.Value,
                },
                 Format = (BarcodeFormat)_formatPort.Value,
                Encoder = new MultiFormatWriter(),
            };
            
            var result = writer.Encode(_codePort.Value);

            _imagePort.Value = new Image(BitMatrixToPixelBuffer(result));
            return new ValueTask<bool>(true);
        }

        private PixelBuffer<L8> BitMatrixToPixelBuffer(BitMatrix matrix){
            var buffer = new PixelBuffer<L8>(matrix.Width, matrix.Height);
            for (int h = 0; h < (int)_heightPort.Value; h++)
            {
                for (int w = 0; w < (int)_widthPort.Value; w++)
                {
                    buffer[w,h] = matrix[w,h] ? new L8(255) : new L8(0);
                }
            }

            return buffer;
        }
    }
}