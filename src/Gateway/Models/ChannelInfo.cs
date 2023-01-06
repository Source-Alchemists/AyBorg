using System.Collections.Concurrent;
using Grpc.Net.Client;

namespace AyBorg.Gateway.Models;
public record ChannelInfo : IDisposable
{
    private bool _disposedValue;

    public string ServiceUniqueName { get; init; } = string.Empty;
    public string TypeName { get; init; } = string.Empty;
    public GrpcChannel Channel { get; init; } = null!;
    public BlockingCollection<Notification> Notifications { get; } = new BlockingCollection<Notification>();
    public bool IsAcceptingNotifications { get; set; } = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Channel?.Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
