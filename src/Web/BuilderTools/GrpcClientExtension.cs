using Ayborg.Gateway.Agent.V1;
using Ayborg.Gateway.Analytics.V1;
using Ayborg.Gateway.Audit.V1;
using Ayborg.Gateway.Cognitive.V1;
using Ayborg.Gateway.V1;
using AyBorg.Web.Services;
using Grpc.Core;

namespace AyBorg.Web;

internal static class GrpcClientExtension
{
    private const string FallbackUrl = "http://localhost:6000";
    private const string GatewayUrlConfig = "AyBorg:Gateway:Url";

    public static WebApplicationBuilder RegisterGrpcClients(this WebApplicationBuilder builder)
    {
        Uri? gatewayUrl = new(builder.Configuration.GetValue(GatewayUrlConfig, FallbackUrl)!);
        // Open endpoints
        CreateClientFactory<Register.RegisterClient>(builder, gatewayUrl, false);
        CreateClientFactory<Notify.NotifyClient>(builder, gatewayUrl, false);
        CreateClientFactory<EventLog.EventLogClient>(builder, gatewayUrl, false);
        // Secured endpoints
        // AyBorg.Agent
        CreateClientFactory<ProjectManagement.ProjectManagementClient>(builder, gatewayUrl);
        CreateClientFactory<ProjectSettings.ProjectSettingsClient>(builder, gatewayUrl);
        CreateClientFactory<Editor.EditorClient>(builder, gatewayUrl);
        CreateClientFactory<Runtime.RuntimeClient>(builder, gatewayUrl);
        CreateClientFactory<Storage.StorageClient>(builder, gatewayUrl);
        CreateClientFactory<DeviceManager.DeviceManagerClient>(builder, gatewayUrl);
        // AyBorg.Audit
        CreateClientFactory<Audit.AuditClient>(builder, gatewayUrl);
        // AyBorg.Cognitive
        CreateClientFactory<ProjectManager.ProjectManagerClient>(builder, gatewayUrl);
        CreateClientFactory<FileManager.FileManagerClient>(builder, gatewayUrl);
        CreateClientFactory<AnnotationManager.AnnotationManagerClient>(builder, gatewayUrl);
        CreateClientFactory<DatasetManager.DatasetManagerClient>(builder, gatewayUrl);
        CreateClientFactory<JobManager.JobManagerClient>(builder, gatewayUrl);
        return builder;
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
                try
                {
                    ITokenProvider tokenProvider = serviceProvider.GetRequiredService<ITokenProvider>();
                    string token = await tokenProvider.GenerateTokenAsync();
                    metaData.Add("Authorization", $"Bearer {token}");
                }
                catch (ObjectDisposedException)
                {
                    // Nothing to do.
                }
            });
        }
    }
}
