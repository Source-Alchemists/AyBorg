using Autodroid.Agent.Services;
using Autodroid.SDK.Authorization;
using Autodroid.SDK.Data.DTOs;
using Autodroid.SDK.Data.Mapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Autodroid.Agent.Controllers;

[ApiController]
[Route("[controller]")]
[JwtAuthorize(Roles = new[] { Roles.Administrator, Roles.Engineer })]
public sealed class FlowController : ControllerBase
{
    private readonly ILogger<FlowController> _logger;
    private readonly IFlowService _flowService;
    private readonly ICacheService _cacheService;
    private readonly IDtoMapper _dtoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowController"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="flowService">The flow service.</param>
    /// <param name="cacheService">The cache service.</param>
    /// <param name="dtoMapper">The dto mapper.</param>
    public FlowController(ILogger<FlowController> logger, IFlowService flowService, ICacheService cacheService, IDtoMapper dtoMapper)
    {
        _logger = logger;
        _flowService = flowService;
        _cacheService = cacheService;
        _dtoMapper = dtoMapper;
    }

    [HttpGet("steps")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async IAsyncEnumerable<StepDto> GetStepsAsync()
    {
        await ValueTask.CompletedTask;
        foreach (var s in _flowService.GetSteps())
        {
            var dto = _dtoMapper.Map(s);
            yield return dto;
        }
    }

    [HttpPost("steps/{stepId}/{x}/{y}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<StepDto>> AddStepAsync(Guid stepId, int x, int y)
    {
        var stepProxy = await _flowService.AddStepAsync(stepId, x, y);
        if (stepProxy == null)
        {
            return NotFound();
        }

        var result = _dtoMapper.Map(stepProxy);
        return Ok(result);
    }

    [HttpDelete("steps/{stepId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult> DeleteStepAsync(Guid stepId)
    {
        if (await _flowService.TryRemoveStepAsync(stepId))
        {
            return Ok();
        }

        return NotFound();
    }

    [HttpPut("steps/{stepId}/{x}/{y}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult> MoveStepAsync(Guid stepId, int x, int y)
    {
        if (await _flowService.TryMoveStepAsync(stepId, x, y))
        {
            return Ok();
        }

        return NotFound();
    }

    [HttpGet("links")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async IAsyncEnumerable<LinkDto> GetLinksAsync()
    {
        await ValueTask.CompletedTask;
        foreach (var l in _flowService.GetLinks())
        {
            var dto = _dtoMapper.Map(l);
            yield return dto;
        }
    }

    [HttpPost("links/{sourcePortId}/{targetPortId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async ValueTask<ActionResult<LinkDto>> LinkPortsAsync(Guid sourcePortId, Guid targetPortId)
    {
        var result = await _flowService.LinkPortsAsync(sourcePortId, targetPortId);
        if (result == null)
        {
            return Conflict();
        }

        return Ok(_dtoMapper.Map(result));
    }

    [HttpDelete("links/{linkId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult> UnlinkPortsAsync(Guid linkId)
    {
        if (await _flowService.TryUnlinkPortsAsync(linkId))
        {
            return Ok();
        }

        return NotFound();
    }

    [HttpGet("ports/{portId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async ValueTask<ActionResult<PortDto>> GetPortAsync(Guid portId)
    {
        var port = await _flowService.GetPortAsync(portId);
        if (port == null)
        {
            return NoContent();
        }

        return Ok(_dtoMapper.Map(port));
    }

    [HttpGet("ports/{portId}/{iterationId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async ValueTask<ActionResult<PortDto>> GetPortFromIterationAsync(Guid portId, Guid iterationId)
    {
        var port = await _flowService.GetPortAsync(portId);
        if (port == null)
        {
            return NoContent();
        }

        return Ok(_cacheService.GetOrCreatePortEntry(iterationId, port));
    }

    [HttpPut("ports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult> UpdatePortAsync(PortDto portDto)
    {
        if (portDto.Value == null)
        {
            _logger.LogWarning("Port value is null");
            return BadRequest();
        }

        if (!await _flowService.TryUpdatePortValueAsync(portDto.Id, portDto.Value))
        {
            return BadRequest();
        }

        return Ok();
    }

    [HttpGet("steps/{stepId}/executiontime/{iterationId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async ValueTask<ActionResult<long>> GetStepExecutionTimeAsync(Guid stepId, Guid iterationId)
    {
        var step = _flowService.GetSteps().FirstOrDefault(s => s.Id == stepId);
        if (step == null)
        {
            return NoContent();
        }

        var entry = _cacheService.GetOrCreateStepExecutionTimeEntry(iterationId, step);
        return await ValueTask.FromResult(Ok(entry));
    }
}