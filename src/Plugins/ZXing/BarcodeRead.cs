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
        private readonly ImagePort _inputImagePort = new("Image", PortDirection.Input, null!);
        private readonly EnumPort _inputEmumPort = new("Barcode Format", PortDirection.Input, BarcodeFormats.All);
        private readonly BooleanPort _allowAutoRotate = new("Allow Auto Rotate", PortDirection.Input, false);
        private readonly BooleanPort _allowTryInvert = new("Allow try Invert", PortDirection.Input, false);
        private readonly BooleanPort _allowTryHarder = new("Allow try Harder", PortDirection.Input, false);
        private readonly StringPort _outputStringPort = new("String", PortDirection.Output, string.Empty);

        public string DefaultName => "Barcode.Read";

        public BarcodeRead(ILogger<BarcodeRead> logger)
        {
            _logger = logger;
            Ports = new IPort[]
            {
                _inputImagePort,
                _inputEmumPort,
                _outputStringPort,
                _allowAutoRotate,
                _allowTryInvert,
                _allowTryHarder
            };
        }

        public IEnumerable<IPort> Ports { get; }

        public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
        {
            ReadOnlySpan<byte> imageBuffer = _inputImagePort.Value.AsPacked<Rgb24>().Buffer;
            var reader = new BarcodeReaderGeneric();
            reader.Options.PureBarcode = true; // if no format is given lib will still search for QR code
            var rgbLumSrc = new RGBLuminanceSource(imageBuffer.ToArray(), _inputImagePort.Value.Width, _inputImagePort.Value.Height);

            reader.Options.PossibleFormats = GetBarcodeFormats(_inputEmumPort.Value);
            reader.AutoRotate = _allowAutoRotate.Value;
            reader.Options.TryInverted = _allowTryInvert.Value;
            reader.Options.TryHarder = _allowTryHarder.Value;

            Result? value = reader.Decode(rgbLumSrc);

            if (value is null)
            {
                _logger.LogWarning("Could not find a barcode.");
                return ValueTask.FromResult(false);
            }

            _outputStringPort.Value = value.Text;
            _logger.LogDebug("Barcode string: '{_outputStringPort.Value}'", _outputStringPort.Value);
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
