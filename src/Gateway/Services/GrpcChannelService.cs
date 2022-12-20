using System.Collections.Concurrent;
using AyBorg.Gateway.Models;
using Grpc.Core;
using Grpc.Net.Client;

namespace AyBorg.Gateway.Services;

public sealed class GrpcChannelService : IGrpcChannelService
{
    private readonly ILogger<GrpcChannelService> _logger;
    private readonly ConcurrentDictionary<string, ChannelInfo> _channels = new();


    public GrpcChannelService(ILogger<GrpcChannelService> logger)
    {
        _logger = logger;
    }

    public bool TryRegisterChannel(string uniqueServiceName, string typeName, string address)
    {
        if (_channels.ContainsKey(uniqueServiceName))
        {
            _logger.LogWarning("Channel for {UniqueServiceName} already registered.", uniqueServiceName);
            return false;
        }

        var channel = GrpcChannel.ForAddress(address);
        return _channels.TryAdd(uniqueServiceName, new ChannelInfo
        {
            ServiceUniqueName = uniqueServiceName,
            TypeName = typeName,
            Channel = channel
        });
    }

    public bool TryUnregisterChannel(string uniqueServiceName)
    {
        if (!_channels.ContainsKey(uniqueServiceName))
        {
            _logger.LogWarning("Channel for {UniqueServiceName} not found.", uniqueServiceName);
            return false;
        }

        bool result = _channels.Remove(uniqueServiceName, out ChannelInfo? channelInfo);
        channelInfo?.Dispose();
        return result;
    }

    public ChannelInfo GetChannelByName(string uniqueServiceName)
    {
        if (!_channels.ContainsKey(uniqueServiceName))
        {
            throw new KeyNotFoundException($"Channel for {uniqueServiceName} not found.");
        }

        return _channels[uniqueServiceName];
    }

    public IEnumerable<ChannelInfo> GetChannelsByTypeName(string typeName)
    {
        IEnumerable<ChannelInfo> keys = _channels.Values.Where(v => v.TypeName.Equals(typeName, StringComparison.InvariantCultureIgnoreCase));
        foreach (ChannelInfo key in keys)
        {
            yield return key;
        }
    }

    public T CreateClient<T>(string uniqueServiceName)
    {
        if (string.IsNullOrEmpty(uniqueServiceName))
        {
            _logger.LogWarning("UniqueServiceName is null or empty");
            throw new RpcException(new Status(StatusCode.InvalidArgument, "UniqueServiceName is null or empty"));
        }

        try
        {
            GrpcChannel channel = GetChannelByName(uniqueServiceName).Channel;
            return (T)Activator.CreateInstance(typeof(T), channel)!;
        }
        catch (KeyNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Agent not found ({uniqueServiceName})"));
        }
    }
}
