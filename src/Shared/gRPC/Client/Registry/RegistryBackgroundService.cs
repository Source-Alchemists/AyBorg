using Ayborg.Gateway.V1;
using AyBorg.SDK.System.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AyBorg.Shared.gRPC.Registry;

public sealed class RegistryBackgroundService : BackgroundService
{
    private readonly ILogger<RegistryBackgroundService> _logger;
    private readonly Register.RegisterClient _registerClient;
    private readonly IServiceConfiguration _serviceConfiguration;
    private Guid _serviceId = Guid.Empty;

    public RegistryBackgroundService(ILogger<RegistryBackgroundService> logger,
                                        Register.RegisterClient registerClient,
                                        IServiceConfiguration serviceConfiguration)
    {
        _logger = logger;
        _registerClient = registerClient;
        _serviceConfiguration = serviceConfiguration;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registry service is starting.");
        try
        {
            await Register(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to register at start", ex);
        }

        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registry service is stopping.");
        try
        {
            StatusResponse response = await _registerClient.UnregisterAsync(new UnregisterRequest
            {
                Id = _serviceId.ToString()
            }, cancellationToken: cancellationToken);

            if (!response.Success)
            {
                _logger.LogWarning("Failed to unregister service: {ErrorMessage}", response.ErrorMessage);
            }

            _serviceId = Guid.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to unregister", ex);
        }

        await base.StopAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.Factory.StartNew(async () =>
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_serviceId != Guid.Empty)
                {
                    StatusResponse response = await _registerClient.HeartbeatAsync(new HeartbeatRequest
                    {
                        Id = _serviceId.ToString()
                    }, cancellationToken: stoppingToken);

                    if (!response.Success)
                    {
                        _logger.LogWarning("Failed to send heartbeat: {ErrorMessage}", response.ErrorMessage);
                    }
                }
                else
                {
                    await Register(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to send heartbeat", ex);
            }

            await Task.Delay(TimeSpan.FromSeconds(30));
        }
    }, TaskCreationOptions.LongRunning);

    private async ValueTask Register(CancellationToken cancellationToken)
    {
        StatusResponse response = await _registerClient.RegisterAsync(new RegisterRequest
        {
            Name = _serviceConfiguration.DisplayName,
            UniqueName = _serviceConfiguration.UniqueName,
            Type = _serviceConfiguration.TypeName,
            Url = _serviceConfiguration.Url,
            Version = _serviceConfiguration.Version
        }, cancellationToken: cancellationToken);

        if (!response.Success)
        {
            _logger.LogError("Failed to register service: {ErrorMessage}", response.ErrorMessage);
        }

        _serviceId = Guid.Parse(response.Id);
    }
}
