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
        private readonly EnumPort _inputEmumPort = new("Barcode Format", PortDirection.Input, BarcodeFormats.Undefined);
        private readonly StringPort _outputStringPort = new("String", PortDirection.Output, String.Empty);

        public string DefaultName => "Barcode.Read";

        public BarcodeRead(ILogger<BarcodeRead> logger)
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
            reader.Options.PureBarcode = true; // if no format is given lib will still search for QR code
            var rgbLumSrc = new RGBLuminanceSource(imageBuffer.ToArray(),_inputImagePort.Value.Width, _inputImagePort.Value.Height );
            

            // possible improvements: input of a list of possible formats
            if(!_inputEmumPort.Value.Equals(BarcodeFormats.Undefined))
            {
                if(Enum.TryParse(_inputEmumPort.Value.ToString(), out BarcodeFormat outFormat))
                {
                    reader.Options.PossibleFormats = new List<BarcodeFormat>(){outFormat};
                }
                else 
                {
                    _logger.LogWarning("Provided Barcode Format is not valid. Please provide a correct search format.");
                    return ValueTask.FromResult(false);
                }
            }
            
            var value = reader.Decode(rgbLumSrc);
            
            if(value is null)
            {
                _logger.LogWarning("Could not find a barcode.");
                return ValueTask.FromResult(false);
            }
            _outputStringPort.Value = value.Text;
            _logger.LogDebug($"Barcode string: '{_outputStringPort.Value}'");
            return ValueTask.FromResult(true);
        }


    }


}