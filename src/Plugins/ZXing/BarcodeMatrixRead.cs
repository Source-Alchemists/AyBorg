using AyBorg.Plugins.ZXing.Models;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.ImageProcessing.Pixels;
using Microsoft.Extensions.Logging;
using ZXing;

namespace AyBorg.Plugins.ZXing
{
    public sealed class BarcodeMatrixRead : IStepBody
    {
        private readonly ILogger<BarcodeMatrixRead> _logger;
        private readonly ImagePort _inputImagePort = new("Image", PortDirection.Input, null!);
        private readonly EnumPort _inputEmumPort = new("Matrix Barcode Format", PortDirection.Input, MatrixBarcodeFormats.Undefined);
        private readonly StringPort _outputStringPort = new("String", PortDirection.Output, String.Empty);

        public string DefaultName => "Barcode.Matrix.Read";

        public BarcodeMatrixRead(ILogger<BarcodeMatrixRead> logger)
        {
            _logger = logger;
            Ports = new IPort[]
            {
                _inputImagePort,
                _inputEmumPort,
                _outputStringPort
            };
        }

        public IEnumerable<IPort> Ports { get; }

        public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
        {
            var imageBuffer = _inputImagePort.Value.AsPacked<Rgb24>().Buffer;
            var reader = new BarcodeReaderGeneric();
            reader.Options.PureBarcode = false;
            reader.Options.TryInverted = true;
            reader.Options.TryHarder = true;
            reader.AutoRotate = true;
            var rgbLumSrc = new RGBLuminanceSource(imageBuffer.ToArray(), _inputImagePort.Value.Width, _inputImagePort.Value.Height);

            if (_inputEmumPort.Value.Equals(MatrixBarcodeFormats.Undefined))
            {
                reader.Options.PossibleFormats = new List<BarcodeFormat>(){
                    BarcodeFormat.AZTEC,
                    BarcodeFormat.DATA_MATRIX,
                    BarcodeFormat.MAXICODE,
                    BarcodeFormat.PDF_417,
                    BarcodeFormat.QR_CODE
                };
            }
            else
            {
                if (Enum.TryParse(_inputEmumPort.Value.ToString(), out BarcodeFormat outFormat))
                {
                    reader.Options.PossibleFormats = new List<BarcodeFormat>() { outFormat };
                }
                else
                {
                    _logger.LogWarning("Provided Matrix Barcode Format is not valid. Please provide a correct search format.");
                    return ValueTask.FromResult(false);
                }
            }

            var value = reader.Decode(rgbLumSrc);
            if (value is null)
            {
                _logger.LogWarning("Could not find a matrix barcode.");
                return ValueTask.FromResult(false);
            }
            _outputStringPort.Value = value.Text;
            _logger.LogDebug($"Matrix barcode string: '{_outputStringPort.Value}'");
            return ValueTask.FromResult(true);
        }
    }
}