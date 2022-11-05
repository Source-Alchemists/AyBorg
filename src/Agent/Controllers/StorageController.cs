using Atomy.Agent.Services;
using Atomy.SDK.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atomy.Agent.Controllers;

[ApiController]
[Route("[controller]")]
[JwtAuthorize(Roles = new[] { Roles.Administrator, Roles.Engineer })]
public sealed class StorageController : ControllerBase
{
    private readonly ILogger<StorageController> _logger;
    private readonly IStorageService _storageService;

    public StorageController(ILogger<StorageController> logger, IStorageService storageService)
    {
        _logger = logger;
        _storageService = storageService;
    }

    [HttpGet("directories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<string[]>> GetDirectories(string path)
    {
        var directories = _storageService.GetDirectories(path);
        return await Task.FromResult(Ok(directories));
    }
}