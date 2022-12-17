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

        builder.Services.AddGrpcClient<AgentProjectManagement.AgentProjectManagementClient>(option =>
        {
            option.Address = new Uri(gatewayUrl!);
        });

        builder.Services.AddGrpcClient<AgentProjectSettings.AgentProjectSettingsClient>(option =>
        {
            option.Address = new Uri(gatewayUrl!);
        });

        builder.Services.AddGrpcClient<AgentEditor.AgentEditorClient>(option =>
        {
            option.Address = new Uri(gatewayUrl!);
        });

        builder.Services.AddGrpcClient<AgentRuntime.AgentRuntimeClient>(option =>
        {
            option.Address = new Uri(gatewayUrl!);
        });
    }
}
