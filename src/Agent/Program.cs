using AyBorg.Agent.Guards;
using AyBorg.Agent.Services;
using AyBorg.Agent.Services.gRPC;
using AyBorg.Data.Agent;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.Common;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.SDK.Communication.gRPC.Registry;
using AyBorg.SDK.Communication.MQTT;
using AyBorg.SDK.System.Agent;
using AyBorg.SDK.System.Configuration;
using AyBorg.SDK.System.Runtime;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string? databaseProvider = builder.Configuration.GetValue("DatabaseProvider", "SqlLite");

builder.Services.AddDbContextFactory<ProjectContext>(options =>
    _ = databaseProvider switch
    {
        "SqlLite" => options.UseSqlite(builder.Configuration.GetConnectionString("SqlLiteConnection"),
                        x => x.MigrationsAssembly("AyBorg.Database.Migrations.SqlLite")),
        "PostgreSql" => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection")!,
                        x => x.MigrationsAssembly("AyBorg.Database.Migrations.PostgreSql")),
        _ => throw new Exception("Invalid database provider")
    }
);

builder.Services.AddAuthorization();
builder.Services.AddGrpc();

builder.Services.AddGrpcClient<Ayborg.Gateway.V1.Register.RegisterClient>(options =>
{
    string? gatewayUrl = builder.Configuration.GetValue("AyBorg:Gateway:Url", "http://localhost:5000");
    options.Address = new Uri(gatewayUrl!);
});

builder.Services.AddGrpcClient<Ayborg.Gateway.Agent.V1.Notify.NotifyClient>(options =>
{
    string? gatewayUrl = builder.Configuration.GetValue("AyBorg:Gateway:Url", "http://localhost:5000");
    options.Address = new Uri(gatewayUrl!);
});

builder.Services.AddHostedService<RegistryBackgroundService>();

// Repositories
builder.Services.AddSingleton<IProjectRepository, ProjectRepository>();

builder.Services.AddSingleton<IEnvironment, AyBorg.SDK.Common.Environment>();
builder.Services.AddSingleton<IServiceConfiguration, ServiceConfiguration>();
builder.Services.AddSingleton<IRuntimeMapper, RuntimeMapper>();
builder.Services.AddSingleton<IRpcMapper, RpcMapper>();
builder.Services.AddSingleton<IRuntimeToStorageMapper, RuntimeToStorageMapper>();
builder.Services.AddSingleton<IRuntimeConverterService, RuntimeConverterService>();
builder.Services.AddSingleton<IPluginsService, PluginsService>();
builder.Services.AddSingleton<IProjectManagementService, ProjectManagementService>();
builder.Services.AddSingleton<IEngineHost, EngineHost>();
builder.Services.AddSingleton<IEngineFactory, EngineFactory>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<INotifyService, NotifyService>();
builder.Services.AddSingleton<IMqttClientProvider, MqttClientProvider>();
builder.Services.AddSingleton<ICommunicationStateProvider, CommunicationStateProvider>();

builder.Services.AddScoped<IJwtConsumer, JwtConsumer>();
builder.Services.AddScoped<IFlowService, FlowService>();

builder.Services.AddTransient<IProjectSettingsService, ProjectSettingsService>();
builder.Services.AddTransient<IStorageService, StorageService>();

WebApplication app = builder.Build();

app.UseAuthorization();
app.UseJwtMiddleware();
app.UseProjectStateGuardMiddleware();

app.MapGrpcService<ProjectManagementServiceV1>();
app.MapGrpcService<ProjectSettingsServiceV1>();
app.MapGrpcService<EditorServiceV1>();
app.MapGrpcService<RuntimeServiceV1>();
app.MapGrpcService<StorageServiceV1>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

// Create database if not exists
app.Services.GetService<IDbContextFactory<ProjectContext>>()!.CreateDbContext().Database.Migrate();

app.Services.GetService<IPluginsService>()?.Load();
await app.Services.GetService<IProjectManagementService>()?.TryLoadActiveAsync().AsTask()!;
await app.Services.GetService<IMqttClientProvider>()?.ConnectAsync().AsTask()!;

app.Run();
