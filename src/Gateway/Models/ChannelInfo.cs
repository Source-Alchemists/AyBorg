using System.Collections.Concurrent;
using Grpc.Net.Client;

namespace AyBorg.Gateway.Models;
public record ChannelInfo : IDisposable
{
    private bool _disposedValue;

    public string ServiceUniqueName { get; init; } = string.Empty;
    public string TypeName { get; init; } = string.Empty;
    public GrpcChannel Channel { get; init; } = null!;
    public ConcurrentQueue<Notification> Notifications { get; } = new ConcurrentQueue<Notification>();

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
