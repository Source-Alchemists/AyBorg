using Ayborg.Gateway.V1;
using AyBorg.Gateway.Models;
using Grpc.Core;

namespace AyBorg.Gateway.Services;

public sealed class RegisterServiceV1 : Register.RegisterBase
{
    private readonly ILogger<RegisterServiceV1> _logger;
    private readonly IKeeperService _keeperService;

    public RegisterServiceV1(ILogger<RegisterServiceV1> logger, IKeeperService keeperService)
    {
        _logger = logger;
        _keeperService = keeperService;
    }

    public override async Task<StatusResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        var newServiceEntry = new ServiceEntry
        {
            Name = request.Name,
            UniqueName = request.UniqueName,
            Type = request.Type,
            Url = request.Url,
            Version = request.Version
        };

        try
        {
            Guid id = await _keeperService.RegisterAsync(newServiceEntry);
            _logger.LogInformation("Registered {Name} ({Url}) with id [{Id}].", newServiceEntry.Name, newServiceEntry.Url, id);
            return new StatusResponse { Success = true, Id = id.ToString(), ErrorMessage = string.Empty };
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to register", ex);
            return new StatusResponse { Success = false, Id = string.Empty, ErrorMessage = "Failed to register" };
        }
    }

    public override async Task<StatusResponse> Unregister(UnregisterRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out Guid id))
            {
                return new StatusResponse { Success = false, Id = request.Id, ErrorMessage = "Invalid id" };
            }

            await _keeperService.UnregisterAsync(id);
            _logger.LogInformation("Unregistered {Id}.", id);
            return new StatusResponse { Success = true, Id = request.Id, ErrorMessage = string.Empty };
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Failed to unregister", ex);
            return new StatusResponse { Success = false, Id = request.Id, ErrorMessage = "Failed to unregister" };
        }
    }

    public override async Task<StatusResponse> Heartbeat(HeartbeatRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out Guid id))
        {
            return new StatusResponse { Success = false, Id = request.Id, ErrorMessage = "Invalid id" };
        }

        IEnumerable<ServiceEntry> knownServices = await _keeperService.GetAllRegistryEntriesAsync();
        ServiceEntry? serviceEntry = knownServices.FirstOrDefault(x => x.Id == id);
        if (serviceEntry == null)
        {
            return new StatusResponse { Success = false, Id = request.Id, ErrorMessage = "Service not found" };
        }

        try
        {
            await _keeperService.UpdateTimestamp(serviceEntry);
            return new StatusResponse { Success = true, Id = request.Id, ErrorMessage = string.Empty };
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Failed to update timestamp", ex);
            return new StatusResponse { Success = false, Id = request.Id, ErrorMessage = "Failed to update timestamp" };
        }
    }

    public override async Task<GetServicesResponse> GetServices(GetServicesRequest request, ServerCallContext context)
    {
        IEnumerable<ServiceEntry> knownServices = await _keeperService.GetAllRegistryEntriesAsync();

        if (!string.IsNullOrEmpty(request.Id))
        {
            if (Guid.TryParse(request.Id, out Guid id))
            {
                knownServices = knownServices.Where(s => s.Id.Equals(id));
            }
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            knownServices = knownServices.Where(s => s.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(request.UniqueName))
        {
            knownServices = knownServices.Where(s => s.UniqueName.Equals(request.UniqueName, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(request.Type))
        {
            knownServices = knownServices.Where(s => s.Type.Equals(request.Type, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(request.Version))
        {
            knownServices = knownServices.Where(s => s.Version.Equals(request.Version, StringComparison.OrdinalIgnoreCase));
        }

        return new GetServicesResponse
        {
            Services = { knownServices.Select(s => new ServiceInfo
            {
                Id = s.Id.ToString(),
                Name = s.Name,
                UniqueName = s.UniqueName,
                Type = s.Type,
                Url = s.Url,
                Version = s.Version
            }) }
        };
    }
}
