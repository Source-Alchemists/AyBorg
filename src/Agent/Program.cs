using Ayborg.Gateway.V1;
using AyBorg.Agent.Guards;
using AyBorg.Agent.Hubs;
using AyBorg.Agent.Services;
using AyBorg.Communication.gRPC.Registry;
using AyBorg.Database.Data;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.Common;
using AyBorg.SDK.Communication.MQTT;
using AyBorg.SDK.Data.Mapper;
using AyBorg.SDK.System.Configuration;
using AyBorg.SDK.System.Runtime;
using Microsoft.AspNetCore.ResponseCompression;
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

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddGrpcClient<Register.RegisterClient>(option =>
{
    string? gatewayUrl = builder.Configuration.GetValue("AyBorg:Gateway:Url", "http://localhost:5000");
    option.Address = new Uri(gatewayUrl!);
});

builder.Services.AddHostedService<RegistryBackgroundService>();

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<IEnvironment, AyBorg.SDK.Common.Environment>();
builder.Services.AddSingleton<IServiceConfiguration, ServiceConfiguration>();
builder.Services.AddSingleton<IDtoMapper, DtoMapper>();
builder.Services.AddSingleton<IRuntimeToStorageMapper, RuntimeToStorageMapper>();
builder.Services.AddSingleton<IRuntimeConverterService, RuntimeConverterService>();
builder.Services.AddSingleton<IPluginsService, PluginsService>();
builder.Services.AddSingleton<IProjectManagementService, ProjectManagementService>();
builder.Services.AddSingleton<IEngineHost, EngineHost>();
builder.Services.AddSingleton<IEngineFactory, EngineFactory>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<IFlowHub, FlowHub>();
builder.Services.AddSingleton<IMqttClientProvider, MqttClientProvider>();
builder.Services.AddSingleton<ICommunicationStateProvider, CommunicationStateProvider>();

builder.Services.AddScoped<IJwtConsumerService, JwtConsumerService>();
builder.Services.AddScoped<IFlowService, FlowService>();
builder.Services.AddScoped<IStorageService, StorageService>();

builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.UseJwtMiddleware();
app.UseProjectStateGuardMiddleware();

app.MapControllers();
app.MapHub<FlowHubContext>("/hubs/flow");

// Create database if not exists
app.Services.GetService<IDbContextFactory<ProjectContext>>()!.CreateDbContext().Database.Migrate();

app.Services.GetService<IPluginsService>()?.Load();
await app.Services.GetService<IProjectManagementService>()?.TryLoadActiveAsync().AsTask()!;
await app.Services.GetService<IMqttClientProvider>()?.ConnectAsync().AsTask()!;

app.Run();
