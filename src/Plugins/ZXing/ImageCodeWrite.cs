using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using ImageTorque;
using ImageTorque.Buffers;
using ImageTorque.Pixels;
using Microsoft.Extensions.Logging;
using ZXing;
using ZXing.Common;

namespace AyBorg.Plugins.ZXing
{
    public class ImageCodeWrite : IStepBody
    {
        private readonly ILogger<ImageCodeWrite> _logger;
        private readonly EnumPort _formatPort = new("Code format", PortDirection.Input, BarcodeFormat.QR_CODE);
        private readonly StringPort _codePort = new("Code", PortDirection.Input, string.Empty);
        private readonly NumericPort _widthPort = new("Width", PortDirection.Input, 100);
        private readonly NumericPort _heightPort = new("Height", PortDirection.Input, 100);
        private readonly NumericPort _marginPort = new("Margin", PortDirection.Input, 0);
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
                _widthPort,
                _heightPort,
            };
        }
        public IEnumerable<IPort> Ports { get; }

        public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken) {      
            _imagePort.Value?.Dispose();

            var writer = new BarcodeWriterGeneric{
                Options = new EncodingOptions{
                    Width = (int)_widthPort.Value,
                    Height = (int)_heightPort.Value,
                    Margin = (int) _marginPort.Value,
                },
                 Format = (BarcodeFormat)_formatPort.Value,
                Encoder = new MultiFormatWriter(),
            };
            
            var result = writer.Encode(_codePort.Value);

            var buffer = BitMatrixToPixelBuffer(result);

            //Todo write image to rgb converter in imageTorque
            // todo throw correct errors when width hight margin is not set correctly

            _imagePort.Value = new Image(buffer);
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