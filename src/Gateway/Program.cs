using AyBorg.Database.Data;
using AyBorg.Gateway.Services;
using AyBorg.Gateway.Services.Agent;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.System.Configuration;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    string? serviceUrl = builder.Configuration.GetValue("AyBorg:Service:Url", string.Empty);
    if (string.IsNullOrEmpty(serviceUrl))
    {
        throw new Exception("Service url is not set");
    }

    bool useTls = serviceUrl.StartsWith("https");
    if (useTls) return; // Nothing to configure as TLS is used by default.

    string portStr = serviceUrl.Substring(serviceUrl.LastIndexOf(":")+1);
    if (!int.TryParse(portStr, out int port))
    {
        throw new Exception("Invalid port");
    }
    options.ListenLocalhost(port, o => o.Protocols = HttpProtocols.Http2);
});

// Add services to the container.
string? databaseProvider = builder.Configuration.GetValue("DatabaseProvider", "SqlLite");

builder.Services.AddDbContextFactory<RegistryContext>(options =>
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

builder.Services.AddSingleton<IGatewayConfiguration, GatewayConfiguration>();
builder.Services.AddSingleton<IGrpcChannelService, GrpcChannelService>();
builder.Services.AddSingleton<IKeeperService, KeeperService>();

builder.Services.AddScoped<IJwtConsumer, JwtConsumer>();

WebApplication app = builder.Build();

app.UseAuthorization();
app.UseJwtMiddleware();

app.MapGrpcService<RegisterServiceV1>();
app.MapGrpcService<ProjectManagementPassthroughServiceV1>();
app.MapGrpcService<ProjectSettingsPassthroughServiceV1>();
app.MapGrpcService<EditorPassthroughServiceV1>();
app.MapGrpcService<RuntimePassthroughServiceV1>();
app.MapGrpcService<StoragePassthroughServiceV1>();
app.MapGrpcService<NotifyPassthroughServiceV1>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

// Create database if not exists
app.Services.GetService<IDbContextFactory<RegistryContext>>()!.CreateDbContext().Database.Migrate();

app.Run();
