using System.Runtime.CompilerServices;
using AyBorg.Plugins.ZXing.Models;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.ImageProcessing.Pixels;
using Microsoft.Extensions.Logging;
using ZXing;

namespace AyBorg.Plugins.ZXing
{
    public sealed class BarcodeRead : IStepBody
    {
        private readonly ILogger<BarcodeRead> _logger;
        private readonly ImagePort _imagePort = new("Image", PortDirection.Input, null!);
        private readonly EnumPort _barcodeFormatPort = new("Barcode format", PortDirection.Input, BarcodeFormats.All);
        private readonly BooleanPort _allowAutoRotatePort = new("Allow auto rotate", PortDirection.Input, false);
        private readonly BooleanPort _allowTryInvertPort = new("Allow try invert", PortDirection.Input, false);
        private readonly BooleanPort _allowTryHarderPort = new("Allow try harder", PortDirection.Input, false);
        private readonly StringPort _codePort = new("Code", PortDirection.Output, string.Empty);

        public string DefaultName => "Barcode.Read";

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
            var reader = new BarcodeReaderGeneric();
            reader.Options.PureBarcode = true; // if no format is given lib will still search for QR code
            var rgbLumSrc = new RGBLuminanceSource(imageBuffer.ToArray(), _imagePort.Value.Width, _imagePort.Value.Height);

            reader.Options.PossibleFormats = GetBarcodeFormats(_barcodeFormatPort.Value);
            reader.AutoRotate = _allowAutoRotatePort.Value;
            reader.Options.TryInverted = _allowTryInvertPort.Value;
            reader.Options.TryHarder = _allowTryHarderPort.Value;

            Result? value = reader.Decode(rgbLumSrc);

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
                return new List<BarcodeFormat> { BarcodeFormat.All_1D };
            }
            else
            {
                _ = Enum.TryParse(enumObj.ToString(), out BarcodeFormat barcodeFormat);
                return new List<BarcodeFormat> { barcodeFormat };
            }
        }
    }
}
