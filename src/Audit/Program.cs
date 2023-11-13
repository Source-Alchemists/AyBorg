using AyBorg.Audit;
using AyBorg.Audit.Services;
using AyBorg.SDK.Communication.gRPC.Registry;
using AyBorg.SDK.Logging.Analytics;
using AyBorg.SDK.System.Configuration;
using Elastic.Apm.AspNetCore;
using Elastic.Apm.GrpcClient;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string serviceUniqueName = builder.Configuration.GetValue("AyBorg:Service:UniqueName", "AyBorg.Audit")!;

builder.Logging.AddOpenTelemetry(options => {
    options.SetResourceBuilder(
        ResourceBuilder.CreateDefault()
        .AddService(serviceUniqueName));
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceUniqueName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation());

// Add services to the container.
builder.Services.AddGrpc();

builder.RegisterGrpcClients();

builder.AddAyBorgAnalyticsLogger();

builder.Services.AddHostedService<RegistryBackgroundService>();

builder.Services.AddSingleton<IServiceConfiguration, ServiceConfiguration>();

builder.Services.AddTransient<AyBorg.Data.Audit.Repositories.Agent.IProjectAuditRepository, AyBorg.Data.Audit.Repositories.Agent.AgentProjectAuditRepository>();
builder.Services.AddTransient<AyBorg.Data.Audit.Repositories.IAuditReportRepository, AyBorg.Data.Audit.Repositories.AuditReportRepository>();
builder.Services.AddTransient<IAgentAuditService, AgentAuditService>();

WebApplication app = builder.Build();

app.UseElasticApm(builder.Configuration, new GrpcClientDiagnosticSubscriber());

// Configure the HTTP request pipeline.
app.MapGrpcService<AuditServiceV1>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
