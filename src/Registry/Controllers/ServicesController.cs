using Autodroid.SDK.Data.DTOs;
using Autodroid.ServiceRegistry.Services;
using Microsoft.AspNetCore.Mvc;

namespace Autodroid.ServiceRegistry.Controllers;

[ApiController]
[Route("[controller]")]
public class ServicesController : ControllerBase
{
    private readonly ILogger<ServicesController> _logger;
    private readonly IKeeperService _keeperService;

    /// <summary>
    /// Initializes a new instance of <see cref="ServicesController"/>.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="keeperService">Keeper service.</param>
    /// <exception cref="ArgumentException">Called if no keeper service is provided.</exception>
    public ServicesController(ILogger<ServicesController> logger, IKeeperService keeperService)
    {
        _logger = logger;
        _keeperService = keeperService ?? throw new ArgumentNullException(nameof(keeperService));
    }

    /// <summary>
    /// Gets services.
    /// </summary>
    /// <param name="serviceRegistryEntry">The service registry entry.</param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status208AlreadyReported)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> RegisterAsync(ServiceRegistryEntryDto serviceRegistryEntry)
    {
        try
        {
            var id = await _keeperService.RegisterAsync(serviceRegistryEntry);
            _logger.LogInformation("Registered {Name} ({Url}) with id [{Id}].", serviceRegistryEntry.Name, serviceRegistryEntry.Url, id);
            return Ok(id);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to register", ex);
            return new StatusCodeResult(StatusCodes.Status208AlreadyReported);
        }
    }

    /// <summary>
    /// Unregister service.
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> UnregisterAsync(Guid id)
    {
        try
        {
            await _keeperService.UnregisterAsync(id);
            _logger.LogInformation("Unregistered {id}.", id);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Failed to unregister", ex);
            return NoContent();
        }
    }

    /// <summary>
    /// Gets service registry entries.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceRegistryEntryDto[]>> GetAsync()
    {
        return Ok(await _keeperService.GetAllServiceRegistryEntriesAsync());
    }

    /// <summary>
    /// Gets service registry entries.
    /// </summary>
    [HttpGet("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceRegistryEntryDto[]>> GetAsync(string name)
    {
        return Ok(await _keeperService.FindServiceRegistryEntriesAsync(name));
    }

    /// <summary>
    /// Gets service registry entries.
    /// </summary>
    [HttpGet("type/{typeName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceRegistryEntryDto[]>> GetByTypeNameAsync(string typeName)
    {
        var services = await _keeperService.GetAllServiceRegistryEntriesAsync();
        return Ok(services.Where(s => s.Type == typeName));
    }

    /// <summary>
    /// Gets service registry entries.
    /// </summary>
    [HttpGet("Id/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceRegistryEntryDto>> GetAsync(Guid id)
    {
        var result = await _keeperService.GetServiceRegistryEntryAsync(id);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Update service registry entry.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Update(ServiceRegistryEntryDto serviceRegistryEntry)
    {
        try
        {
            await _keeperService.UpdateTimestamp(serviceRegistryEntry);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Failed to update", ex);
            return NoContent();
        }
    }
}