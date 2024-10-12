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

using AyBorg.Communication;
using AyBorg.Log.Services;
using AyBorg.Data.Log;
using AyBorg.Communication.gRPC.Registry;

using Elastic.Extensions.Logging;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string serviceUniqueName = builder.Configuration.GetValue("AyBorg:Service:UniqueName", "AyBorg.Log")!;
bool isOpenTelemetryEnabled = builder.Configuration.GetValue("OpenTelemetry:Enabled", false)!;
bool isElasticApmEnabled = builder.Configuration.GetValue("ElasticApm:Enabled", false)!;

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

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddGrpcClient<Ayborg.Gateway.V1.Register.RegisterClient>(options =>
{
    string? gatewayUrl = builder.Configuration.GetValue("AyBorg:Gateway:Url", "http://localhost:5000");
    Console.WriteLine($"Gateway connection: {gatewayUrl}");
    options.Address = new Uri(gatewayUrl!);
});

builder.Services.AddHostedService<RegistryBackgroundService>();

builder.Services.AddSingleton<IServiceConfiguration, ServiceConfiguration>();
builder.Services.AddSingleton<IEventLogRepository, EventLogRepository>();
builder.Services.AddSingleton<IEventStorage, EventStorage>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<EventLogServiceV1>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
