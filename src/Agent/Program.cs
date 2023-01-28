using AyBorg.Agent;
using AyBorg.Agent.Guards;
using AyBorg.Agent.Services;
using AyBorg.Agent.Services.gRPC;
using AyBorg.Data.Agent;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.Common;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.SDK.Communication.gRPC.Registry;
using AyBorg.SDK.Communication.MQTT;
using AyBorg.SDK.Logging.Analytics;
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
                        x => x.MigrationsAssembly("AyBorg.Data.Agent.Migrations.SqlLite")),
        "PostgreSql" => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection")!,
                        x => x.MigrationsAssembly("AyBorg.Data.Agent.Migrations.PostgreSql")),
        _ => throw new Exception("Invalid database provider")
    }
);

builder.Services.AddAuthorization();
builder.Services.AddGrpc();

builder.RegisterGrpcClients();

builder.AddAyBorgAnalyticsLogger();

builder.Services.AddHostedService<RegistryBackgroundService>();

builder.Services.AddSingleton<IPluginsService, PluginsService>();
builder.Services.AddSingleton<IEngineHost, EngineHost>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<IMqttClientProvider, MqttClientProvider>();
builder.Services.AddSingleton<ICommunicationStateProvider, CommunicationStateProvider>();

builder.Services.AddTransient<IJwtConsumer, JwtConsumer>();
// Repositories
builder.Services.AddTransient<IProjectRepository, ProjectRepository>();
// Environment / Configuration
builder.Services.AddTransient<IEnvironment, AyBorg.SDK.Common.Environment>();
builder.Services.AddTransient<IServiceConfiguration, ServiceConfiguration>();
// Mapper / Converter
builder.Services.AddTransient<IRuntimeMapper, RuntimeMapper>();
builder.Services.AddTransient<IRpcMapper, RpcMapper>();
builder.Services.AddTransient<IRuntimeToStorageMapper, RuntimeToStorageMapper>();
builder.Services.AddTransient<IRuntimeConverterService, RuntimeConverterService>();
// Runtime / Project
builder.Services.AddTransient<IFlowService, FlowService>();
builder.Services.AddTransient<IEngineFactory, EngineFactory>();
builder.Services.AddTransient<IStorageService, StorageService>();
builder.Services.AddTransient<INotifyService, NotifyService>();
builder.Services.AddTransient<IProjectManagementService, ProjectManagementService>();
builder.Services.AddTransient<IProjectSettingsService, ProjectSettingsService>();
// Audit
builder.Services.AddTransient<AuditMapper>();
builder.Services.AddTransient<IAuditProviderService, AuditProviderService>();

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
