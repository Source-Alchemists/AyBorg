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

using Ayborg.Gateway.Analytics.V1;
using AyBorg.Data.Gateway;
using AyBorg.Gateway.Services;
using AyBorg.Gateway.Services.Agent;
using AyBorg.Gateway.Services.Analytics;
using AyBorg.Gateway.Services.Audit;
using AyBorg.Logging;
using AyBorg.Communication;
using AyBorg.Authorization;

using Microsoft.EntityFrameworkCore;

using Elastic.Extensions.Logging;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.IdentityModel.Tokens.Jwt;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string serviceUniqueName = builder.Configuration.GetValue("AyBorg:Service:UniqueName", "AyBorg.Gateway")!;
bool isOpenTelemetryEnabled = builder.Configuration.GetValue("OpenTelemetry:Enabled", false)!;
bool isElasticApmEnabled = builder.Configuration.GetValue("ElasticApm:Enabled", false)!;

builder.Services.Configure<SecurityOptions>(builder.Configuration.GetSection("Security"));

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

if(isElasticApmEnabled)
{
    builder.Services.AddAllElasticApm();
    builder.Logging.AddElasticsearch();
}

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
    options.Address = new Uri(builder.Configuration.GetValue("Kestrel:Endpoints:gRPC:Url", "http://localhost:6000")!);
});

builder.AddAyBorgAnalyticsLogger();

builder.Services.AddSingleton<IServiceConfiguration, ServiceConfiguration>();
builder.Services.AddSingleton<IGatewayConfiguration, GatewayConfiguration>();
builder.Services.AddSingleton<IGrpcChannelService, GrpcChannelService>();
builder.Services.AddSingleton<IKeeperService, KeeperService>();

builder.Services.AddScoped<ITokenValidator<JwtSecurityToken>, JwtValidator>();

WebApplication app = builder.Build();

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
// Net
app.MapGrpcService<AyBorg.Gateway.Services.Cognitive.FileManagerPassthroughServiceV1>();
app.MapGrpcService<AyBorg.Gateway.Services.Cognitive.ProjectManagerPassthroughServiceV1>();
app.MapGrpcService<AyBorg.Gateway.Services.Cognitive.AnnotationManagerPassthroughServiceV1>();
app.MapGrpcService<AyBorg.Gateway.Services.Cognitive.DatasetManagerPassthroughServiceV1>();
app.MapGrpcService<AyBorg.Gateway.Services.Cognitive.JobManagerPassthroughServiceV1>();
// Net.Agent
app.MapGrpcService<AyBorg.Gateway.Services.Cognitive.Agent.StatusManagerPassthroughServiceV1>();
app.MapGrpcService<AyBorg.Gateway.Services.Cognitive.Agent.JobManagerPassthroughServiceV1>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

// Create database if not exists
app.Services.GetService<IDbContextFactory<RegistryContext>>()!.CreateDbContext().Database.Migrate();

app.Run();
