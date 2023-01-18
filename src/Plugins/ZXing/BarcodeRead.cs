using System.Runtime.CompilerServices;
using AyBorg.Plugins.ZXing.Models;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using ImageTorque.Pixels;
using Microsoft.Extensions.Logging;
using ZXing;

namespace AyBorg.Plugins.ZXing
{
    public sealed class BarcodeRead : IStepBody
    {
        private readonly ILogger<BarcodeRead> _logger;
        private readonly ImagePort _imagePort = new("Image", PortDirection.Input, null!);
        private readonly EnumPort _barcodeFormatPort = new("Barcode format", PortDirection.Input, BarcodeFormats.All);
        private readonly BooleanPort _allowAutoRotatePort = new("Auto rotate", PortDirection.Input, false);
        private readonly BooleanPort _allowTryInvertPort = new("Auto invert", PortDirection.Input, false);
        private readonly BooleanPort _allowTryHarderPort = new("Harder", PortDirection.Input, false);
        private readonly StringPort _codePort = new("Code", PortDirection.Output, string.Empty);
        private readonly BarcodeReaderGeneric _nativBarcodeReader = new();

        private byte[] _tmpBuffer = null!;

        public string DefaultName => "Barcode.Read";

        public IEnumerable<string> Categories { get; } = new List<string> { DefaultStepCategories.ImageProcessing };

        public BarcodeRead(ILogger<BarcodeRead> logger)
        {
            _logger = logger;
            Ports = new IPort[]
            {
                _imagePort,
                _barcodeFormatPort,
                _codePort,
                _allowAutoRotatePort,
                _allowTryInvertPort,
                _allowTryHarderPort
            };
        }

        public IEnumerable<IPort> Ports { get; }

        public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
        {
            ReadOnlySpan<byte> imageBuffer = _imagePort.Value.AsPacked<Rgb24>().Buffer;
            if (_tmpBuffer == null || _tmpBuffer.Length != imageBuffer.Length)
            {
                _tmpBuffer = new byte[imageBuffer.Length];
            }
            imageBuffer.CopyTo(_tmpBuffer.AsSpan());
            _nativBarcodeReader.Options.PureBarcode = true;
            var rgbLumSrc = new RGBLuminanceSource(_tmpBuffer, _imagePort.Value.Width, _imagePort.Value.Height);

            _nativBarcodeReader.Options.PossibleFormats = GetBarcodeFormats(_barcodeFormatPort.Value);
            _nativBarcodeReader.AutoRotate = _allowAutoRotatePort.Value;
            _nativBarcodeReader.Options.TryInverted = _allowTryInvertPort.Value;
            _nativBarcodeReader.Options.TryHarder = _allowTryHarderPort.Value;

            Result? value = _nativBarcodeReader.Decode(rgbLumSrc);

            if (value is null)
            {
                _logger.LogWarning("Could not find a barcode.");
                return ValueTask.FromResult(false);
            }

            _codePort.Value = value.Text;
            _logger.LogDebug("Barcode string: '{_codePort.Value}'", _codePort.Value);
            return ValueTask.FromResult(true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IList<BarcodeFormat> GetBarcodeFormats(Enum enumObj)
        {
            // possible improvements: input of a list of possible formats
            if (enumObj.Equals(BarcodeFormats.All))
            {
                // Warning: Mistake in zxing - ALL_1D value from native BarcodeFormat from zxing does not select all 1d barcode formats that are available 
                return new List<BarcodeFormat>(){
                    BarcodeFormat.CODABAR,
                    BarcodeFormat.CODE_39,
                    BarcodeFormat.CODE_93,
                    BarcodeFormat.CODE_128,
                    BarcodeFormat.EAN_8,
                    BarcodeFormat.EAN_13,
                    BarcodeFormat.ITF,
                    BarcodeFormat.RSS_14,
                    BarcodeFormat.UPC_A,
                    BarcodeFormat.UPC_E,
                    BarcodeFormat.UPC_EAN_EXTENSION,
                    BarcodeFormat.MSI,
                    BarcodeFormat.PLESSEY,
                    BarcodeFormat.IMB,
                    BarcodeFormat.PHARMA_CODE
                };
            }
            else
            {
                _ = Enum.TryParse(enumObj.ToString(), out BarcodeFormat barcodeFormat);
                return new List<BarcodeFormat> { barcodeFormat };
            }
        }
    }
}
