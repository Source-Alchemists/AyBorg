using Ayborg.Gateway.Agent.V1;
using Ayborg.Gateway.Analytics.V1;
using Ayborg.Gateway.V1;
using AyBorg.Web.Services;
using Grpc.Core;

namespace AyBorg.Web.BuilderTools;

internal static class GrpcClientRegisterTool
{
    private const string FallbackUrl = "http://localhost:5000";
    private const string GatewayUrlConfig = "AyBorg:Gateway:Url";

    public static void Register(WebApplicationBuilder builder)
    {
        Uri? gatewayUrl = new(builder.Configuration.GetValue(GatewayUrlConfig, FallbackUrl)!);
        // Open endpoints
        CreateClientFactory<Register.RegisterClient>(builder, gatewayUrl, false);
        CreateClientFactory<Notify.NotifyClient>(builder, gatewayUrl, false);
        CreateClientFactory<EventLog.EventLogClient>(builder, gatewayUrl, false);
        // Secured endpoints
        CreateClientFactory<ProjectManagement.ProjectManagementClient>(builder, gatewayUrl);
        CreateClientFactory<ProjectSettings.ProjectSettingsClient>(builder, gatewayUrl);
        CreateClientFactory<Editor.EditorClient>(builder, gatewayUrl);
        CreateClientFactory<Runtime.RuntimeClient>(builder, gatewayUrl);
        CreateClientFactory<Storage.StorageClient>(builder, gatewayUrl);
    }

    private static void CreateClientFactory<T>(WebApplicationBuilder builder, Uri uri, bool tokenRequired = true) where T : ClientBase
    {
        IHttpClientBuilder httpClientBuilder = builder.Services.AddGrpcClient<T>(option =>
        {
            option.ChannelOptionsActions.Add(o => o.UnsafeUseInsecureChannelCallCredentials = true);
            option.Address = uri;

        });

        if (tokenRequired)
        {
            httpClientBuilder.AddCallCredentials(async (context, metaData, serviceProvider) =>
            {
                ITokenProvider tokenProvider = serviceProvider.GetRequiredService<ITokenProvider>();
                string token = await tokenProvider.GenerateTokenAsync();
                metaData.Add("Authorization", $"Bearer {token}");
            });
        }
    }
}
