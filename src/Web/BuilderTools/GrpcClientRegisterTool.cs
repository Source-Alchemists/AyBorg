using Ayborg.Gateway.Agent.V1;
using Ayborg.Gateway.V1;

namespace AyBorg.Web.BuilderTools;

internal static class GrpcClientRegisterTool
{
    private const string FallbackUrl = "http://localhost:5000";
    private const string GatewayUrlConfig = "AyBorg:Gateway:Url";
    public static void Register(WebApplicationBuilder builder)
    {
        string? gatewayUrl = builder.Configuration.GetValue(GatewayUrlConfig, FallbackUrl);

        builder.Services.AddGrpcClient<Register.RegisterClient>(option =>
        {
            option.Address = new Uri(gatewayUrl!);
        });

        builder.Services.AddGrpcClient<ProjectManagement.ProjectManagementClient>(option =>
        {
            option.Address = new Uri(gatewayUrl!);
        });

        builder.Services.AddGrpcClient<ProjectSettings.ProjectSettingsClient>(option =>
        {
            option.Address = new Uri(gatewayUrl!);
        });

        builder.Services.AddGrpcClient<Editor.EditorClient>(option =>
        {
            option.Address = new Uri(gatewayUrl!);
        });

        builder.Services.AddGrpcClient<Runtime.RuntimeClient>(option =>
        {
            option.Address = new Uri(gatewayUrl!);
        });

        builder.Services.AddGrpcClient<Storage.StorageClient>(option =>
        {
            option.Address = new Uri(gatewayUrl!);
        });
    }
}
