using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using Microsoft.Extensions.Logging;

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
            return ValueTask.FromResult(true);
        }
    }
}