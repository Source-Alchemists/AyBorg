using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.ImageProcessing.Pixels;
using Microsoft.Extensions.Logging;
using OpenCvSharp;

namespace AyBorg.Plugins.OpenCvSharp
{
    public sealed class QrCodeRead : IStepBody
    {
        private readonly ILogger<QrCodeRead> _logger;
        private readonly ImagePort _inputImagePort = new("Image", PortDirection.Input, null!);
        private readonly StringPort _outputStringPort = new("String", PortDirection.Output, String.Empty);

        public string DefaultName => "QrCode.Read";

        public QrCodeRead(ILogger<QrCodeRead> logger)
        {
            _logger = logger;
            Ports = new IPort[]
            {
                _inputImagePort,
                _outputStringPort

            };
        }

        public IEnumerable<IPort> Ports { get; }


        public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
        {
  
            using var mat = Mat.FromImageData(_inputImagePort.Value.AsPacked<Rgb24>().Buffer);
            var qrDetector = new QRCodeDetector();
            qrDetector.Detect(mat, out var points);
            var value = qrDetector.Decode(mat,points);
            

            return ValueTask.FromResult(true);
        }
    }
}