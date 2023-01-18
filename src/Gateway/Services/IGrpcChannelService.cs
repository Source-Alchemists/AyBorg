using AyBorg.Gateway.Models;

namespace AyBorg.Gateway.Services;

public interface IGrpcChannelService
{
    bool TryRegisterChannel(string uniqueServiceName, string typeName, string address);
    bool TryUnregisterChannel(string uniqueServiceName);
    ChannelInfo GetChannelByName(string uniqueServiceName);
    IEnumerable<ChannelInfo> GetChannelsByTypeName(string typeName);
    T CreateClient<T>(string uniqueServiceName);
}
