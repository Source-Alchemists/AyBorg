using AyBorg.Hub.Connect;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();
builder.Services.AddGrpc();

WebApplication app = builder.Build();
app.UseAuthorization();
// Configure the HTTP request pipeline.
app.AddAyBorgAgentServer();
app.UseForwardedHeaders();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
