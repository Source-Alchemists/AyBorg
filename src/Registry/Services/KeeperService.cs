using System.Reflection;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Atomy.ServiceRegistry.Models;
using Atomy.SDK.DTOs;
using Atomy.ServiceRegistry.Mapper;
using Atomy.Database.Data;

namespace Atomy.ServiceRegistry.Services;

public sealed class KeeperService : IKeeperService, IDisposable
{
    private const int HeartbeatPollingTimeMs = 1000;
    private const int HeartbeatValidationTimeoutMs = 60000;
    private readonly BlockingCollection<ServiceEntry> _availableServices = new BlockingCollection<ServiceEntry>();
    private readonly Task _heartbeatTask;
    private readonly ILogger<KeeperService> _logger;
    private readonly IDalMapper _dalMapper;
    private readonly IDbContextFactory<RegistryContext> _registryContextFactory;
    private readonly BlockingCollection<ServiceRegistryEntryDto> _registryEntries = new BlockingCollection<ServiceRegistryEntryDto>();
    private readonly ServiceRegistryEntryDto _selfServiceEntry;
    private bool _isDisposed = false;
    private bool _isHeartbeatTaskTerminated = false;

    /// <summary>
    /// Initializes a new instance of <see cref="KeeperService"/>.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="dalMapper">The dal mapper.</param>
    /// <param name="registryContextFactory">The registry context.</param>
    public KeeperService(ILogger<KeeperService> logger, IConfiguration configuration, IDalMapper dalMapper, IDbContextFactory<RegistryContext> registryContextFactory)
    {
        _logger = logger;
        _dalMapper = dalMapper;
        _registryContextFactory = registryContextFactory;

        var assembly = Assembly.GetEntryAssembly();
        var version = assembly?.GetName()?.Version;
        var versionString = "unknown";
        if (version != null)
        {
            versionString = version.ToString();
        }
        var serverUrl = configuration.GetValue<string>("Kestrel:Endpoints:Https:Url");
        if (serverUrl == null || serverUrl.Equals(string.Empty))
        {
            serverUrl = configuration.GetValue<string>("Kestrel:Endpoints:Http:Url");
        }

        _selfServiceEntry = new ServiceRegistryEntryDto
        {
            Id = Guid.NewGuid(),
            Name = "Atomy.ServiceRegistry",
            UniqueName = "Atomy.ServiceRegistry",
            Type = "Atomy.ServiceRegistry",
            Url = serverUrl,
            Version = versionString
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
    public async Task<IEnumerable<ServiceRegistryEntryDto>> FindServiceRegistryEntriesAsync(string name)
    {

        var result = _registryEntries.Where(x => x.Name.Equals(name));
        if (_selfServiceEntry.Name.Equals(name))
        {
            result.Append(_selfServiceEntry);
        }
        await Task.CompletedTask;
        return result;
    }

    /// <summary>
    /// Get all service registry entries.
    /// </summary>
    /// <returns>All service registry entries.</returns>
    public async Task<IEnumerable<ServiceRegistryEntryDto>> GetAllServiceRegistryEntriesAsync()
    {
        return await Task.Run(() =>
        {
            var result = new List<ServiceRegistryEntryDto>
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
    public async Task<ServiceRegistryEntryDto?> GetServiceRegistryEntryAsync(Guid serviceId)
    {
        var result = _registryEntries.FirstOrDefault(x => x.Id == serviceId);
        await Task.CompletedTask;
        return result;
    }

    /// <summary>
    /// Register a new service.
    /// </summary>
    /// <param name="serviceRegistryEntry">Service registry entry.</param>
    /// <returns>Id for the new service.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the same service instance is added multiple times.</exception>
    public async Task<Guid> RegisterAsync(ServiceRegistryEntryDto serviceRegistryEntry)
    {
        var matchingService = FindAvailableService(serviceRegistryEntry);

        if (matchingService != null)
        {
            throw new InvalidOperationException($"Adding the same service instance multiple times is not allowed ({serviceRegistryEntry.Name} | {serviceRegistryEntry.Url})!");
        }

        using var context = await _registryContextFactory.CreateDbContextAsync();
        var knownService = await context.ServiceEntries!.FirstOrDefaultAsync(x => x.UniqueName == serviceRegistryEntry.UniqueName && x.Type == serviceRegistryEntry.Type);
        ServiceEntry serviceEntry;
        if (knownService != null)
        {
            // Identified existing service by unique name and type.
            // Update address, port and name, as they may has changed.
            knownService.Url = serviceRegistryEntry.Url;
            knownService.Name = serviceRegistryEntry.Name;
            serviceEntry = _dalMapper.Map(knownService);
            serviceEntry.LastConnectionTime = DateTime.UtcNow;
            _logger.LogTrace($"Service {serviceEntry.Name} ({serviceEntry.Url}) is already registered and will be used with same id [{serviceEntry.Id}].");
        }
        else
        {
            if (await context.ServiceEntries!.AnyAsync(x => x.UniqueName == serviceRegistryEntry.UniqueName))
            {
                throw new InvalidOperationException($"Adding a service with unique name ({serviceRegistryEntry.UniqueName} is not allowed! A service with the name is already registered.");
            }
            serviceEntry = _dalMapper.Map(serviceRegistryEntry);
            serviceEntry.Id = Guid.NewGuid();
            await context.ServiceEntries!.AddAsync(serviceEntry);
        }

        await context.SaveChangesAsync();
        serviceRegistryEntry.Id = serviceEntry.Id;
        _availableServices.Add(serviceEntry);
        _registryEntries.Add(serviceRegistryEntry);

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

        await Task.CompletedTask;
    }

    /// <summary>
    /// Updates the service timestamp.
    /// If not updated frequencly, the service will be recognized as not available and be removed to available service collection.
    /// </summary>
    /// <param name="serviceRegistryEntry">The desired service.</param>
    /// <returns>Task.</returns>
    public async Task UpdateTimestamp(ServiceRegistryEntryDto serviceRegistryEntry)
    {
        var matchingService = FindAvailableService(serviceRegistryEntry);

        if (matchingService == null)
        {
            throw new KeyNotFoundException($"Service with url '{serviceRegistryEntry.Url}' not found!");
        }

        matchingService.LastConnectionTime = DateTime.UtcNow;

        await Task.CompletedTask;
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

    private ServiceEntry? FindAvailableService(ServiceRegistryEntryDto serviceRegistryEntry) => _availableServices.FirstOrDefault(x => x.Url.Equals(serviceRegistryEntry.Url));

    private ServiceEntry? FindAvailableService(Guid id) => _availableServices.FirstOrDefault(x => x.Id.Equals(id));

    private void RemoveService(Guid serviceId)
    {
        var tmpCollection = new List<ServiceEntry>();
        ServiceEntry? removedEntry = null;
        while (_availableServices.AsEnumerable().Count() > 0)
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
            RemoveServiceRegistryEntry(removedEntry);
        }
    }

    private void RemoveServiceRegistryEntry(ServiceEntry serviceEntry)
    {
        var tmpCollection = new List<ServiceRegistryEntryDto>();
        while (_registryEntries.AsEnumerable().Count() > 0)
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
                        _logger.LogWarning($"Service '{serviceItem.Name}' with id '{serviceItem.Id}' time out and will be removed!");
                        RemoveService(serviceItem.Id);
                        _logger.LogInformation($"Service '{serviceItem.Name}' (Url: '{serviceItem.Url}') removed!");
                    }
                }

                await Task.Delay(HeartbeatPollingTimeMs);
            }

        }, TaskCreationOptions.LongRunning);
    }
}