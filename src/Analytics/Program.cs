using AyBorg.Analytics.Services;
using AyBorg.Data.Analytics;
using AyBorg.SDK.Communication.gRPC.Registry;
using AyBorg.SDK.System.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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
