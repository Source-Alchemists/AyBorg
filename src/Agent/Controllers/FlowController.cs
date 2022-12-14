using AyBorg.Agent.Services;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.Data.DTOs;
using AyBorg.SDK.Data.Mapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AyBorg.Agent.Controllers;

[ApiController]
[Route("[controller]")]
[JwtAuthorize(Roles = new[] { Roles.Administrator, Roles.Engineer, Roles.Reviewer })]
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
        foreach (SDK.Common.IStepProxy s in _flowService.GetSteps())
        {
            StepDto dto = _dtoMapper.Map(s);
            yield return dto;
        }
    }

    [HttpPost("steps/{stepId}/{x}/{y}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<StepDto>> AddStepAsync(Guid stepId, int x, int y)
    {
        SDK.Common.IStepProxy stepProxy = await _flowService.AddStepAsync(stepId, x, y);
        if (stepProxy == null)
        {
            return NotFound();
        }

        StepDto result = _dtoMapper.Map(stepProxy);
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
        foreach (SDK.Common.Ports.PortLink l in _flowService.GetLinks())
        {
            LinkDto dto = _dtoMapper.Map(l);
            yield return dto;
        }
    }

    [HttpPost("links/{sourcePortId}/{targetPortId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async ValueTask<ActionResult<LinkDto>> LinkPortsAsync(Guid sourcePortId, Guid targetPortId)
    {
        SDK.Common.Ports.PortLink result = await _flowService.LinkPortsAsync(sourcePortId, targetPortId);
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
        SDK.Common.Ports.IPort port = await _flowService.GetPortAsync(portId);
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
        SDK.Common.Ports.IPort port = await _flowService.GetPortAsync(portId);
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
        SDK.Common.IStepProxy? step = _flowService.GetSteps().FirstOrDefault(s => s.Id == stepId);
        if (step == null)
        {
            return NoContent();
        }

        long entry = _cacheService.GetOrCreateStepExecutionTimeEntry(iterationId, step);
        return await ValueTask.FromResult(Ok(entry));
    }
}
