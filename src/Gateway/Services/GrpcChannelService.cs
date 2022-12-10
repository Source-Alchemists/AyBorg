using System.Collections.Concurrent;
using Grpc.Net.Client;

namespace AyBorg.Gateway.Services;

public sealed class GrpcChannelService : IGrpcChannelService
{
    private readonly ILogger<GrpcChannelService> _logger;
    private readonly ConcurrentDictionary<string, GrpcChannel> _channels = new();


    public GrpcChannelService(ILogger<GrpcChannelService> logger)
    {
        _logger = logger;
    }

    public bool TryRegisterChannel(string uniqueServiceName, string address)
    {
        if (_channels.ContainsKey(uniqueServiceName))
        {
            _logger.LogWarning("Channel for {UniqueServiceName} already registered.", uniqueServiceName);
            return false;
        }

        var channel = GrpcChannel.ForAddress(address);
        return _channels.TryAdd(uniqueServiceName, channel);
    }

    public bool TryUnregisterChannel(string uniqueServiceName)
    {
        if (!_channels.ContainsKey(uniqueServiceName))
        {
            _logger.LogWarning("Channel for {UniqueServiceName} not found.", uniqueServiceName);
            return false;
        }

        bool result = _channels.Remove(uniqueServiceName, out GrpcChannel? channel);
        channel?.Dispose();
        return result;
    }

    public GrpcChannel GetChannel(string uniqueServiceName)
    {
        if (!_channels.ContainsKey(uniqueServiceName))
        {
            throw new KeyNotFoundException($"Channel for {uniqueServiceName} not found.");
        }

        return _channels[uniqueServiceName];
    }
}
