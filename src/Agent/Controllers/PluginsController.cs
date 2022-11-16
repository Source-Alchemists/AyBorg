using AyBorg.Agent.Services;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.Data.DTOs;
using AyBorg.SDK.Data.Mapper;
using Microsoft.AspNetCore.Mvc;

namespace AyBorg.Agent.Controllers;

[ApiController]
[Route("[controller]")]
[JwtAuthorize(Roles = new[] { Roles.Administrator, Roles.Engineer, Roles.Reviewer })]
public sealed class PluginsController : ControllerBase
{
    private readonly IPluginsService _pluginsService;
    private readonly IDtoMapper _dtoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginsController"/> class.
    /// </summary>
    /// <param name="pluginsService">The plugins service.</param>
    /// <param name="dtoMapper">The dto mapper.</param>
    public PluginsController(IPluginsService pluginsService, IDtoMapper dtoMapper)
    {
        _pluginsService = pluginsService;
        _dtoMapper = dtoMapper;
    }

    /// <summary>
    /// Get steps async.
    /// </summary>
    [HttpGet("Steps")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<IEnumerable<StepDto>>> GetStepsAsync()
    {
        IEnumerable<SDK.Common.IStepProxy> plugins = _pluginsService.Steps;
        var pluginDtos = new List<StepDto>();

        foreach (SDK.Common.IStepProxy p in plugins)
        {
            StepDto dto = _dtoMapper.Map(p);
            pluginDtos.Add(dto);
        }

        return await ValueTask.FromResult(Ok(pluginDtos));
    }
}
