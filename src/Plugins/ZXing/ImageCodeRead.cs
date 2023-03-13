using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using AyBorg.Plugins.ZXing.Models;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using ImageTorque.Pixels;
using Microsoft.Extensions.Logging;
using ZXing;

namespace AyBorg.Plugins.ZXing
{
    public sealed class ImageCodeRead : IStepBody
    {
        private readonly ILogger<ImageCodeRead> _logger;
        private readonly ImagePort _imagePort = new("Image", PortDirection.Input, null!);
        private readonly EnumPort _formatPort = new("Code format", PortDirection.Input, CodeFormats.All);
        private readonly BooleanPort _allowAutoRotatePort = new("Auto rotate", PortDirection.Input, false);
        private readonly BooleanPort _allowTryInvertPort = new("Auto invert", PortDirection.Input, false);
        private readonly BooleanPort _allowTryHarderPort = new("Harder", PortDirection.Input, false);
        private readonly StringCollectionPort _codesPort = new("Codes", PortDirection.Output, new ReadOnlyCollection<string>(new List<string>()));
        private readonly BarcodeReaderGeneric _nativeReader = new();

        private byte[] _tmpBuffer = null!;

        public string DefaultName => "Image.Code.Read";

        public IEnumerable<string> Categories { get; } = new List<string> { DefaultStepCategories.ImageProcessing };

        public ImageCodeRead(ILogger<ImageCodeRead> logger)
        {
            _logger = logger;
            Ports = new IPort[]
            {
                _imagePort,
                _formatPort,
                _codesPort,
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
            var rgbLumSrc = new RGBLuminanceSource(_tmpBuffer, _imagePort.Value.Width, _imagePort.Value.Height);

            _nativeReader.Options.PossibleFormats = GetBarcodeFormats(_formatPort.Value);
            _nativeReader.AutoRotate = _allowAutoRotatePort.Value;
            _nativeReader.Options.TryInverted = _allowTryInvertPort.Value;
            _nativeReader.Options.TryHarder = _allowTryHarderPort.Value;

            Result[]? value = _nativeReader.DecodeMultiple(rgbLumSrc);

            if (value is null)
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace(new EventId((int)EventLogType.Result), "Could not find a code.");
                }
                return ValueTask.FromResult(false);
            }

            _codesPort.Value = new ReadOnlyCollection<string>(value.Select(v => v.Text).ToList());
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(new EventId((int)EventLogType.Result), "Code string: '{_codePort.Value}'", _codesPort.Value);
            }
            return ValueTask.FromResult(true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IList<BarcodeFormat> GetBarcodeFormats(Enum enumObj)
        {
            if (enumObj.Equals(CodeFormats.All_Barcodes) || enumObj.Equals(CodeFormats.All_MatrixBarcodes) || (enumObj.Equals(CodeFormats.All)))
            {
                return Enum.GetValues(typeof(CodeFormats))
                    .Cast<CodeFormats>()
                    .Where(flag => ((CodeFormats)enumObj).HasFlag(flag))
                    .Select(flag => { _ = Enum.TryParse(flag.ToString(), out BarcodeFormat c); return c; })
                    .ToList();
            }
            else
            {
                _ = Enum.TryParse(enumObj.ToString(), out BarcodeFormat barcodeFormat);
                return new List<BarcodeFormat> { barcodeFormat };
            }
        }
    }
}
