/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Microsoft.EntityFrameworkCore;

using AyBorg.Agent;
using AyBorg.Agent.Guards;
using AyBorg.Agent.Result;
using AyBorg.Agent.Services;
using AyBorg.Agent.Services.gRPC;
using AyBorg.Data.Agent;
using AyBorg.Data.Mapper;
using AyBorg.Logging;
using AyBorg.Communication;
using AyBorg.Communication.gRPC.Registry;
using AyBorg.Types;
using AyBorg.Types.Result;
using AyBorg.Authorization;
using AyBorg.Runtime;
using AyBorg.Communication.gRPC;
using AyBorg.Communication.Hub;

using Elastic.Extensions.Logging;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.IdentityModel.Tokens.Jwt;
using AyBorg.Agent.Services.Hub;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string serviceUniqueName = builder.Configuration.GetValue("AyBorg:Service:UniqueName", "AyBorg.Agent")!;
bool isOpenTelemetryEnabled = builder.Configuration.GetValue("OpenTelemetry:Enabled", false)!;
bool isElasticApmEnabled = builder.Configuration.GetValue("ElasticApm:Enabled", false)!;

builder.Services.Configure<SecurityOptions>(builder.Configuration.GetSection("Security"));
builder.Services.Configure<ServiceOptions>(builder.Configuration.GetSection("AyBorg:Service"));
builder.Services.Configure<HubOptions>(builder.Configuration.GetSection("AyBorg:Hub"));

// Add services to the container.
string? databaseProvider = builder.Configuration.GetValue("DatabaseProvider", "SqlLite");

if (isOpenTelemetryEnabled)
{
    builder.Services.AddOpenTelemetry().WithTracing(builder => builder.AddAspNetCoreInstrumentation())
        .ConfigureResource(resource => resource.AddService(serviceUniqueName))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation())
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation());
}

if (isElasticApmEnabled)
{
    builder.Services.AddAllElasticApm();
    builder.Logging.AddElasticsearch();
}

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

builder.Services.AddDbContextFactory<DeviceContext>(options =>
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

builder.Services.AddSingleton<IHubClient, HubClient>();
builder.Services.AddSingleton<ITokenGenerator, JwtGenerator>();

builder.Services.AddHostedService<RegistryBackgroundService>();

builder.Services.AddSingleton<IPluginsService, PluginsService>();
builder.Services.AddSingleton<IEngineHost, EngineHost>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<ICommunicationStateProvider, CommunicationStateProvider>();
builder.Services.AddSingleton<IDeviceManager, DeviceManager>();
builder.Services.AddSingleton<IResultStorageProvider, ResultStorageProvider>();

builder.Services.AddScoped<ITokenValidator<JwtSecurityToken>, JwtValidator>();
// Repositories
builder.Services.AddTransient<IProjectRepository, ProjectRepository>();
builder.Services.AddTransient<IDeviceRepository, DeviceRepository>();
// Environment / Configuration
builder.Services.AddSingleton<IEnvironment, AyBorg.Runtime.Environment>();
builder.Services.AddTransient<IServiceConfiguration, ServiceConfiguration>();
// Mapper / Converter
builder.Services.AddTransient<IRuntimeMapper, RuntimeMapper>();
builder.Services.AddTransient<IRpcMapper, RpcMapper>();
builder.Services.AddTransient<IFlowToStorageMapper, FlowToStorageMapper>();
builder.Services.AddTransient<IDeviceToStorageMapper, DeviceToStorageMapper>();
builder.Services.AddTransient<IRuntimeConverterService, RuntimeConverterService>();
// Runtime / Project
builder.Services.AddTransient<IFlowService, FlowService>();
builder.Services.AddSingleton<IDeviceProxyManagerService, DeviceProxyManagerService>();
builder.Services.AddTransient<IEngineFactory, EngineFactory>();
builder.Services.AddTransient<IStorageService, StorageService>();
builder.Services.AddTransient<INotifyService, NotifyService>();
builder.Services.AddTransient<IProjectManagementService, ProjectManagementService>();
builder.Services.AddTransient<IProjectSettingsService, ProjectSettingsService>();
// Audit
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
app.MapGrpcService<DeviceManagerServiceV1>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

// Create database if not exists
await app.Services.GetService<IDbContextFactory<ProjectContext>>()!.CreateDbContext().Database.MigrateAsync();
await app.Services.GetService<IDbContextFactory<DeviceContext>>()!.CreateDbContext().Database.MigrateAsync();

app.UseAyBorgHub<IHubClient>();

await app.Services.GetService<IPluginsService>()!.LoadAsync().AsTask()!;
await app.Services.GetService<IDeviceProxyManagerService>()!.LoadAsync().AsTask()!;
await app.Services.GetService<IProjectManagementService>()!.TryLoadActiveAsync().AsTask()!;

await app.RunAsync();
