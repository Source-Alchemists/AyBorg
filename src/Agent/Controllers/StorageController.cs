using AyBorg.Agent.Services;
using AyBorg.SDK.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AyBorg.Agent.Controllers;

[ApiController]
[Route("[controller]")]
[JwtAuthorize(Roles = new[] { Roles.Administrator, Roles.Engineer, Roles.Reviewer })]
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
        IEnumerable<string> directories = _storageService.GetDirectories(path);
        return await ValueTask.FromResult(Ok(directories));
    }
}
