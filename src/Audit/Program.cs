using AyBorg.Audit;
using AyBorg.Audit.Services;
using AyBorg.SDK.Communication.gRPC.Registry;
using AyBorg.SDK.Logging.Analytics;
using AyBorg.SDK.System.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.RegisterGrpcClients();

builder.AddAyBorgAnalyticsLogger();

builder.Services.AddHostedService<RegistryBackgroundService>();

builder.Services.AddSingleton<IServiceConfiguration, ServiceConfiguration>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<AuditServiceV1>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
