using Ayborg.Gateway.V1;
using Ayborg.Gateway.Analytics.V1;
using Grpc.Core;

namespace AyBorg.Result;

internal static class GrpcClientExtension
{
    private const string FallbackUrl = "http://localhost:5000";
    private const string GatewayUrlConfig = "AyBorg:Gateway:Url";

    public static WebApplicationBuilder RegisterGrpcClients(this WebApplicationBuilder builder)
    {
        Uri? gatewayUrl = new(builder.Configuration.GetValue(GatewayUrlConfig, FallbackUrl)!);
        // Open endpoints
        CreateClientFactory<Register.RegisterClient>(builder, gatewayUrl);
        CreateClientFactory<EventLog.EventLogClient>(builder, gatewayUrl);
        return builder;
    }

    private static void CreateClientFactory<T>(WebApplicationBuilder builder, Uri uri) where T : ClientBase
    {
        builder.Services.AddGrpcClient<T>(option =>
        {
            option.ChannelOptionsActions.Add(o => o.UnsafeUseInsecureChannelCallCredentials = true);
            option.Address = uri;
        });
    }
}
