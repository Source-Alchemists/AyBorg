using Autodroid.Agent.Services;
using Autodroid.SDK.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Autodroid.Agent.Controllers;

[ApiController]
[Route("[controller]")]
[JwtAuthorize(Roles = new[] { Roles.Administrator, Roles.Engineer })]
public sealed class StorageController : ControllerBase
{
    private readonly IStorageService _storageService;

    public StorageController(IStorageService storageService)
    {
        _storageService = storageService;
    }

    [HttpGet("directories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<string[]>> GetDirectories(string path)
    {
        var directories = _storageService.GetDirectories(path);
        return await ValueTask.FromResult(Ok(directories));
    }
}