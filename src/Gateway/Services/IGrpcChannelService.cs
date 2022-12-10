using Grpc.Net.Client;

namespace AyBorg.Gateway.Services;

public interface IGrpcChannelService
{
    bool TryRegisterChannel(string uniqueServiceName, string address);
    bool TryUnregisterChannel(string uniqueServiceName);
    GrpcChannel GetChannel(string uniqueServiceName);
}
