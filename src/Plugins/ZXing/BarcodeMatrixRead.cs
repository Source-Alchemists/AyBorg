using System.Runtime.CompilerServices;
using AyBorg.Plugins.ZXing.Models;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using ImageTorque.Pixels;
using Microsoft.Extensions.Logging;
using ZXing;

namespace AyBorg.Plugins.ZXing
{
    public sealed class BarcodeMatrixRead : IStepBody
    {
        private readonly ILogger<BarcodeMatrixRead> _logger;
        private readonly ImagePort _imagePort = new("Image", PortDirection.Input, null!);
        private readonly EnumPort _matrixBarcodeFormatPort = new("Matrix barcode format", PortDirection.Input, MatrixBarcodeFormats.All);
        private readonly StringPort _codePort = new("Code", PortDirection.Output, String.Empty);
        private readonly BooleanPort _allowAutoRotatePort = new("Auto rotate", PortDirection.Input, false);
        private readonly BooleanPort _allowTryInvertPort = new("Auto invert", PortDirection.Input, false);
        private readonly BooleanPort _allowTryHarderPort = new("Harder", PortDirection.Input, false);
        private readonly BarcodeReaderGeneric _nativBarcodeReader = new();
        
        private byte[] _tmpBuffer = null!;

        public string DefaultName => "Barcode.Matrix.Read";
        
        public IEnumerable<string> Categories { get; } = new List<string> { DefaultStepCategories.ImageProcessing };
        
        public BarcodeMatrixRead(ILogger<BarcodeMatrixRead> logger)
        {
            _logger = logger;
            Ports = new IPort[]
            {
                _imagePort,
                _matrixBarcodeFormatPort,
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
            _nativBarcodeReader.Options.PureBarcode = false;
            var rgbLumSrc = new RGBLuminanceSource(_tmpBuffer, _imagePort.Value.Width, _imagePort.Value.Height);

            _nativBarcodeReader.Options.PossibleFormats = GetBarcodeFormats(_matrixBarcodeFormatPort.Value);
            _nativBarcodeReader.AutoRotate = _allowAutoRotatePort.Value;
            _nativBarcodeReader.Options.TryInverted = _allowTryInvertPort.Value;
            _nativBarcodeReader.Options.TryHarder = _allowTryHarderPort.Value;


            Result? value = _nativBarcodeReader.Decode(rgbLumSrc);

            if (value is null)
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace(new EventId((int)EventLogType.Result), "Could not find a matrix barcode.");
                }
                return ValueTask.FromResult(false);
            }

            _codePort.Value = value.Text;
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(new EventId((int)EventLogType.Result), "Matrix barcode string: '{_codePort.Value}'", _codePort.Value);
            } 
            return ValueTask.FromResult(true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IList<BarcodeFormat> GetBarcodeFormats(Enum enumObj)
        {
            // possible improvements: input of a list of possible formats
            if (enumObj.Equals(MatrixBarcodeFormats.All))
            {
                return new List<BarcodeFormat>(){
                    BarcodeFormat.AZTEC,
                    BarcodeFormat.DATA_MATRIX,
                    BarcodeFormat.MAXICODE,
                    BarcodeFormat.PDF_417,
                    BarcodeFormat.QR_CODE
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