using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Atomy.Agent.Hubs;
using Atomy.Agent.Services;
using Atomy.SDK.System.Mapper;
using Atomy.Database.Data;
using Atomy.SDK.Authorization;
using Atomy.SDK.System.Services;
using Atomy.SDK.Common;
using Atomy.SDK.Communication.MQTT;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var databaseProvider = builder.Configuration.GetValue("DatabaseProvider", "SqlLite");

builder.Services.AddDbContextFactory<ProjectContext>(options =>
    _ = databaseProvider switch
    {
        "SqlLite" => options.UseSqlite(builder.Configuration.GetConnectionString("SqlLiteConnection"),
                        x => x.MigrationsAssembly("Atomy.Database.Migrations.SqlLite")),
        "PostgreSql" => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection"),
                        x => x.MigrationsAssembly("Atomy.Database.Migrations.PostgreSql")),
        _ => throw new Exception("Invalid database provider")
    }
);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<RegistryService>();
builder.Services.AddHostedService<RegistryService>();

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<IEnvironment, Atomy.SDK.Common.Environment>();
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

builder.Services.AddScoped<IJwtConsumerService, JwtConsumerService>();
builder.Services.AddScoped<IFlowService, FlowService>();
builder.Services.AddScoped<IStorageService, StorageService>();

builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.UseJwtMiddleware();

app.MapControllers();
app.MapHub<FlowHubContext>("/hubs/flow");

// Create database if not exists
app.Services.GetService<IDbContextFactory<ProjectContext>>()!.CreateDbContext().Database.Migrate();

app.Services.GetService<IPluginsService>()?.Load();
app.Services.GetService<IProjectManagementService>()?.TryLoadActiveProjectAsync().Wait();
app.Services.GetService<IMqttClientProvider>()?.ConnectAsync().Wait();

app.Run();
