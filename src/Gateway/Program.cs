using Ayborg.Gateway.Analytics.V1;
using AyBorg.Data.Gateway;
using AyBorg.Gateway.Services;
using AyBorg.Gateway.Services.Agent;
using AyBorg.Gateway.Services.Analytics;
using AyBorg.Gateway.Services.Audit;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.Logging.Analytics;
using AyBorg.SDK.System.Configuration;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string? databaseProvider = builder.Configuration.GetValue("DatabaseProvider", "SqlLite");

builder.Services.AddDbContextFactory<RegistryContext>(options =>
    _ = databaseProvider switch
    {
        "SqlLite" => options.UseSqlite(builder.Configuration.GetConnectionString("SqlLiteConnection"),
                        x => x.MigrationsAssembly("AyBorg.Data.Gateway.Migrations.SqlLite")),
        "PostgreSql" => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection")!,
                        x => x.MigrationsAssembly("AyBorg.Data.Gateway.Migrations.PostgreSql")),
        _ => throw new Exception("Invalid database provider")
    }
);

builder.Services.AddAuthorization();
builder.Services.AddGrpc();

builder.Services.AddGrpcClient<EventLog.EventLogClient>(options =>
{
    options.ChannelOptionsActions.Add(o => o.UnsafeUseInsecureChannelCallCredentials = true);
    options.Address = new Uri(builder.Configuration.GetValue("AyBorg:Service:Url", "http://localhost:5000")!);
});

builder.AddAyBorgAnalyticsLogger();

builder.Services.AddSingleton<IServiceConfiguration, ServiceConfiguration>();
builder.Services.AddSingleton<IGatewayConfiguration, GatewayConfiguration>();
builder.Services.AddSingleton<IGrpcChannelService, GrpcChannelService>();
builder.Services.AddSingleton<IKeeperService, KeeperService>();

builder.Services.AddScoped<IJwtConsumer, JwtConsumer>();

WebApplication app = builder.Build();

Console.WriteLine("Running with following settings:");
foreach (KeyValuePair<string, string?> config in builder.Configuration.AsEnumerable())
{
    Console.WriteLine($"{config.Key} = {config.Value}");
}

app.UseAuthorization();
app.UseJwtMiddleware();

// Gateway
app.MapGrpcService<RegisterServiceV1>();
// Agent
app.MapGrpcService<ProjectManagementPassthroughServiceV1>();
app.MapGrpcService<ProjectSettingsPassthroughServiceV1>();
app.MapGrpcService<EditorPassthroughServiceV1>();
app.MapGrpcService<RuntimePassthroughServiceV1>();
app.MapGrpcService<StoragePassthroughServiceV1>();
app.MapGrpcService<NotifyPassthroughServiceV1>();
app.MapGrpcService<DeviceManagerPassthroughServiceV1>();
// Analytics
app.MapGrpcService<EventLogPassthroughServiceV1>();
// Audit
app.MapGrpcService<AuditPassthroughServiceV1>();
// Result
app.MapGrpcService<AyBorg.Gateway.Services.Result.StoragePassthroughServiceV1>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

// Create database if not exists
app.Services.GetService<IDbContextFactory<RegistryContext>>()!.CreateDbContext().Database.Migrate();

app.Run();
