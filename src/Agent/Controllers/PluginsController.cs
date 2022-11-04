using Atomy.Agent.Services;
using Atomy.SDK.Authorization;
using Atomy.SDK.Data.DTOs;
using Atomy.SDK.Data.Mapper;
using Microsoft.AspNetCore.Mvc;

namespace Atomy.Agent.Controllers;

[ApiController]
[Route("[controller]")]
[JwtAuthorize(Roles = new[] { Roles.Administrator, Roles.Engineer })]
public sealed class PluginsController : ControllerBase
{
	private readonly ILogger<PluginsController> _logger;
	private readonly IPluginsService _pluginsService;
	private readonly IDtoMapper _dtoMapper;

	/// <summary>
	/// Initializes a new instance of the <see cref="PluginsController"/> class.
	/// </summary>
	/// <param name="logger">The logger.</param>
	/// <param name="pluginsService">The plugins service.</param>
	/// <param name="dtoMapper">The dto mapper.</param>
	public PluginsController(ILogger<PluginsController> logger, IPluginsService pluginsService, IDtoMapper dtoMapper)
	{
		_logger = logger;
		_pluginsService = pluginsService;
		_dtoMapper = dtoMapper;
	}

	/// <summary>
	/// Get steps async.
	/// </summary>
	[HttpGet("Steps")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StepDto>>> GetStepsAsync()
	{
		var plugins = _pluginsService.Steps;
		var pluginDtos = new List<StepDto>();

		foreach(var p in plugins)
		{
			var dto = _dtoMapper.Map(p);
			pluginDtos.Add(dto);
		}

		await Task.CompletedTask;
		return Ok(pluginDtos);
	}
}
