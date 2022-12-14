using System.Collections.Concurrent;
using AyBorg.Database.Data;
using AyBorg.Registry.Mapper;
using AyBorg.Registry.Models;
using AyBorg.SDK.Data.DTOs;
using AyBorg.SDK.System.Configuration;
using Microsoft.EntityFrameworkCore;

namespace AyBorg.Registry.Services;

public sealed class KeeperService : IKeeperService, IDisposable
{
    private const int HeartbeatPollingTimeMs = 1000;
    private const int HeartbeatValidationTimeoutMs = 60000;
    private readonly BlockingCollection<ServiceEntry> _availableServices = new();
    private readonly Task _heartbeatTask;
    private readonly ILogger<KeeperService> _logger;
    private readonly IDalMapper _dalMapper;
    private readonly IDbContextFactory<RegistryContext> _registryContextFactory;
    private readonly BlockingCollection<RegistryEntryDto> _registryEntries = new();
    private readonly RegistryEntryDto _selfServiceEntry;
    private bool _isDisposed = false;
    private bool _isHeartbeatTaskTerminated = false;

    /// <summary>
    /// Initializes a new instance of <see cref="KeeperService"/>.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="dalMapper">The dal mapper.</param>
    /// <param name="registryContextFactory">The registry context.</param>
    public KeeperService(ILogger<KeeperService> logger, IConfiguration configuration, IRegistryConfiguration registryConfiguration, IDalMapper dalMapper, IDbContextFactory<RegistryContext> registryContextFactory)
    {
        _logger = logger;
        _dalMapper = dalMapper;
        _registryContextFactory = registryContextFactory;

        var serverUrl = configuration.GetValue<string>("Kestrel:Endpoints:Https:Url");
        if (serverUrl == null || serverUrl.Equals(string.Empty))
        {
            serverUrl = configuration.GetValue<string>("Kestrel:Endpoints:Http:Url");
        }

        if (string.IsNullOrEmpty(serverUrl))
        {
            _logger.LogError("Server url is not set in configuration. (Hint: Kestrel:Endpoints:Https:Url or Kestrel:Endpoints:Http:Url)");
            throw new InvalidOperationException("Server url is not set in configuration.");
        }

        _selfServiceEntry = new RegistryEntryDto
        {
            Id = Guid.NewGuid(),
            Name = registryConfiguration.DisplayName,
            UniqueName = registryConfiguration.UniqueName,
            Type = registryConfiguration.TypeName,
            Url = serverUrl,
            Version = registryConfiguration.Version
        };
        _heartbeatTask = StartHeartbeatsValidation();
    }

    /// <summary>
    /// Dispose all unneeded resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finds service registry entries by name.
    /// </summary>
    /// <param name="name">The searched name.</param>
    /// <returns>Array of entries. Empty array if no entry match the name.</returns>
    public async ValueTask<IEnumerable<RegistryEntryDto>> FindRegistryEntriesAsync(string name)
    {

        var result = _registryEntries.Where(x => x.Name.Equals(name));
        if (_selfServiceEntry.Name.Equals(name))
        {
            result = result.Append(_selfServiceEntry);
        }
        return await ValueTask.FromResult(result);
    }

    /// <summary>
    /// Get all service registry entries.
    /// </summary>
    /// <returns>All service registry entries.</returns>
    public async ValueTask<IEnumerable<RegistryEntryDto>> GetAllRegistryEntriesAsync()
    {
        return await Task.Run(() =>
        {
            var result = new List<RegistryEntryDto>
            {
                _selfServiceEntry
            };
            result.AddRange(_registryEntries);

            return result;
        });
    }

    /// <summary>
    /// Gets the service registry entry asynchronous.
    /// </summary>
    /// <param name="serviceId">The service identifier.</param>
    /// <returns></returns>
    public async ValueTask<RegistryEntryDto?> GetRegistryEntryAsync(Guid serviceId)
    {
        var result = _registryEntries.FirstOrDefault(x => x.Id == serviceId);
        return await ValueTask.FromResult(result);
    }

    /// <summary>
    /// Register a new service.
    /// </summary>
    /// <param name="RegistryEntry">Service registry entry.</param>
    /// <returns>Id for the new service.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the same service instance is added multiple times.</exception>
    public async Task<Guid> RegisterAsync(RegistryEntryDto RegistryEntry)
    {
        var matchingService = FindAvailableService(RegistryEntry);

        if (matchingService != null)
        {
            throw new InvalidOperationException($"Adding the same service instance multiple times is not allowed ({RegistryEntry.Name} | {RegistryEntry.Url})!");
        }

        using var context = await _registryContextFactory.CreateDbContextAsync();
        var knownService = await context.ServiceEntries!.FirstOrDefaultAsync(x => x.UniqueName == RegistryEntry.UniqueName && x.Type == RegistryEntry.Type);
        ServiceEntry serviceEntry;
        if (knownService != null)
        {
            // Identified existing service by unique name and type.
            // Update address, port and name, as they may has changed.
            knownService.Url = RegistryEntry.Url;
            knownService.Name = RegistryEntry.Name;
            serviceEntry = _dalMapper.Map(knownService);
            serviceEntry.LastConnectionTime = DateTime.UtcNow;
            _logger.LogTrace("Service {serviceEntry.Name} ({serviceEntry.Url}) is already registered and will be used with same id [{serviceEntry.Id}].", serviceEntry.Name, serviceEntry.Url, serviceEntry.Id);
        }
        else
        {
            if (await context.ServiceEntries!.AnyAsync(x => x.UniqueName == RegistryEntry.UniqueName))
            {
                throw new InvalidOperationException($"Adding a service with unique name ({RegistryEntry.UniqueName} is not allowed! A service with the name is already registered.");
            }
            serviceEntry = _dalMapper.Map(RegistryEntry);
            serviceEntry.Id = Guid.NewGuid();
            await context.ServiceEntries!.AddAsync(serviceEntry);
        }

        await context.SaveChangesAsync();
        RegistryEntry.Id = serviceEntry.Id;
        _availableServices.Add(serviceEntry);
        _registryEntries.Add(RegistryEntry);

        return serviceEntry.Id;
    }

    /// <summary>
    /// Unregister a service.
    /// </summary>
    /// <param name="serviceId">Service id.</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Thrown if the specified service is not found.</exception>
    public async Task UnregisterAsync(Guid serviceId)
    {
        var matchingService = FindAvailableService(serviceId);

        if (matchingService == null)
        {
            throw new KeyNotFoundException($"Service with id '{serviceId}' not found!");
        }

        RemoveService(serviceId);
        await ValueTask.CompletedTask;
    }

    /// <summary>
    /// Updates the service timestamp.
    /// If not updated frequencly, the service will be recognized as not available and be removed to available service collection.
    /// </summary>
    /// <param name="RegistryEntry">The desired service.</param>
    /// <returns>Task.</returns>
    public async ValueTask UpdateTimestamp(RegistryEntryDto RegistryEntry)
    {
        var matchingService = FindAvailableService(RegistryEntry);

        if (matchingService == null)
        {
            throw new KeyNotFoundException($"Service with url '{RegistryEntry.Url}' not found!");
        }

        matchingService.LastConnectionTime = DateTime.UtcNow;

        await ValueTask.CompletedTask;
    }
    private async void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _isHeartbeatTaskTerminated = true;
                await _heartbeatTask;
                _heartbeatTask.Dispose();
                _availableServices.Dispose();
                _registryEntries.Dispose();
            }

            _isDisposed = true;
        }
    }

    private ServiceEntry? FindAvailableService(RegistryEntryDto RegistryEntry) => _availableServices.FirstOrDefault(x => x.Url.Equals(RegistryEntry.Url));

    private ServiceEntry? FindAvailableService(Guid id) => _availableServices.FirstOrDefault(x => x.Id.Equals(id));

    private void RemoveService(Guid serviceId)
    {
        var tmpCollection = new List<ServiceEntry>();
        ServiceEntry? removedEntry = null;
        while (_availableServices.AsEnumerable().Any())
        {
            var lastEntry = _availableServices.Take();
            if (!lastEntry.Id.Equals(serviceId))
            {
                tmpCollection.Add(lastEntry);
            }
            else
            {
                removedEntry = lastEntry;
            }
        }

        foreach (var tmpItem in tmpCollection)
        {
            _availableServices.Add(tmpItem);
        }

        if (removedEntry != null)
        {
            RemoveRegistryEntry(removedEntry);
        }
    }

    private void RemoveRegistryEntry(ServiceEntry serviceEntry)
    {
        var tmpCollection = new List<RegistryEntryDto>();
        while (_registryEntries.AsEnumerable().Any())
        {
            var lastEntry = _registryEntries.Take();
            if (!(lastEntry.Url.Equals(serviceEntry.Url)))
            {
                tmpCollection.Add(lastEntry);
            }
        }

        foreach (var tmpItem in tmpCollection)
        {
            _registryEntries.Add(tmpItem);
        }
    }

    private Task StartHeartbeatsValidation()
    {
        return Task.Factory.StartNew(async () =>
        {
            while (!_isHeartbeatTaskTerminated)
            {
                foreach (var serviceItem in _availableServices.AsEnumerable())
                {
                    var utcNow = DateTime.UtcNow;
                    var utcHeartbeat = serviceItem.LastConnectionTime.AddMilliseconds(HeartbeatValidationTimeoutMs);
                    if ((utcHeartbeat - utcNow).TotalMilliseconds < 0)
                    {
                        _logger.LogWarning("Service '{serviceItem.Name}' with id '{serviceItem.Id}' time out and will be removed!", serviceItem.Name, serviceItem.Id);
                        RemoveService(serviceItem.Id);
                        _logger.LogInformation("Service '{serviceItem.Name}' (Url: '{serviceItem.Url}') removed!", serviceItem.Name, serviceItem.Url);
                    }
                }

                await Task.Delay(HeartbeatPollingTimeMs);
            }

        }, TaskCreationOptions.LongRunning);
    }
}