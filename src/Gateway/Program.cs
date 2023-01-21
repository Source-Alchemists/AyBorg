using AyBorg.Data.Gateway;
using AyBorg.Gateway.Services;
using AyBorg.Gateway.Services.Agent;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.System.Configuration;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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
