using AyBorg.Agent.Services;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.System.Runtime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AyBorg.Agent.Controllers;

[ApiController]
[Route("[controller]")]
[JwtAuthorize(Roles = new[] { Roles.Administrator, Roles.Engineer, Roles.Reviewer })]
public sealed class RuntimeController : ControllerBase
{
    private readonly ILogger<RuntimeController> _logger;
    private readonly IEngineHost _engineHost;

    public RuntimeController(ILogger<RuntimeController> logger, IEngineHost engineHost)
    {
        _logger = logger;
        _engineHost = engineHost;
    }

    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [AllowAnonymous]
    public async ValueTask<ActionResult<EngineMeta>> GetStatusAsync()
    {
        EngineMeta status = await _engineHost.GetEngineStatusAsync();
        if (status == null)
        {
            _logger.LogWarning("No engine status found.");
            return NoContent();
        }

        return Ok(status);
    }

    [HttpPost("start/{executionType}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public async ValueTask<ActionResult<EngineMeta>> StartRunAsync(EngineExecutionType executionType)
    {
        EngineMeta status = await _engineHost.StartRunAsync(executionType);
        if (status != null)
        {
            return Ok(status);
        }

        _logger.LogWarning("Engine status not modified.");
        return StatusCode(StatusCodes.Status304NotModified);
    }

    [HttpPost("stop")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public async ValueTask<ActionResult<EngineMeta>> StopRunAsync()
    {
        EngineMeta status = await _engineHost.StopRunAsync();
        if (status != null)
        {
            return Ok(status);
        }

        _logger.LogWarning("Engine status not modified.");
        return StatusCode(StatusCodes.Status304NotModified);
    }

    [HttpPost("abort")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public async ValueTask<ActionResult<EngineMeta>> AbortRunAsync()
    {
        EngineMeta status = await _engineHost.AbortRunAsync();
        if (status != null)
        {
            return Ok(status);
        }

        _logger.LogWarning("Engine status not modified.");
        return StatusCode(StatusCodes.Status304NotModified);
    }
}
