using AyBorg.Agent.Services;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.Data.DTOs;
using AyBorg.SDK.Data.Mapper;
using AyBorg.SDK.Projects;
using AyBorg.SDK.System.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace AyBorg.Agent.Controllers;

[ApiController]
[Route("[controller]")]
[JwtAuthorize(Roles = new[] { Roles.Administrator, Roles.Engineer })]
public sealed class ProjectsController : ControllerBase
{
    private readonly ILogger<ProjectsController> _logger;
    private readonly IProjectManagementService _projectManagementService;
    private readonly IDtoMapper _storageToDtoMapper;
    private readonly string _serviceUniqueName;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectsController"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceConfiguration">The service configuration.</param>
    /// <param name="projectManagementService">The project management service.</param>
    /// <param name="storageToDtoMapper">The storage to dto mapper.</param>
    public ProjectsController(ILogger<ProjectsController> logger, IServiceConfiguration serviceConfiguration, IProjectManagementService projectManagementService, IDtoMapper storageToDtoMapper)
    {
        _logger = logger;
        _projectManagementService = projectManagementService;
        _storageToDtoMapper = storageToDtoMapper;
        _serviceUniqueName = serviceConfiguration.UniqueName;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async IAsyncEnumerable<ProjectMetaDto> GetAsync()
    {
        foreach (var metaGroup in (await _projectManagementService.GetAllMetasAsync()).Where(x => x.ServiceUniqueName == _serviceUniqueName).GroupBy(p => p.Id))
        {
            var activeMeta = metaGroup.FirstOrDefault(g => g.IsActive);
            if (activeMeta != null)
            {
                yield return _storageToDtoMapper.Map(activeMeta);
            }
            else
            {
                var meta = metaGroup.OrderByDescending(x => x.UpdatedDate).First();
                yield return _storageToDtoMapper.Map(meta);
            }
        }
    }

    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<ProjectMetaDto>> GetActiveAsync()
    {
        var metas = await _projectManagementService.GetAllMetasAsync();
        var activeProjectMeta = metas.FirstOrDefault(p => p.IsActive && p.ServiceUniqueName == _serviceUniqueName);
        if (activeProjectMeta == null)
        {
            _logger.LogWarning("No active project found.");
            return NotFound();
        }

        var projectMetaDto = _storageToDtoMapper.Map(activeProjectMeta);
        return Ok(projectMetaDto);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<ProjectMetaDto>> CreateAsync(string name)
    {
        var record = await _projectManagementService.CreateAsync(name);
        return Ok(_storageToDtoMapper.Map(record.Meta));
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult> DeleteAsync(Guid id)
    {
        var result = await _projectManagementService.TryDeleteAsync(id);
        return result.IsSuccessful ? Ok() : NotFound(result.Message);
    }

    [HttpPut("{dbId}/active/{isActive}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult> ActivateAsync(Guid dbId, bool isActive)
    {
        var result = await _projectManagementService.TryActivateAsync(dbId, isActive);
        return result.IsSuccessful ? Ok() : NotFound(result.Message);
    }

    [HttpPut("{id}/state/{state}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async ValueTask<ActionResult> ChangeStateAsync(Guid id, ProjectState state)
    {
        var result = await _projectManagementService.TryChangeStateAsync(id, state);
        return result.IsSuccessful ? Ok() : Conflict(result.Message);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async ValueTask<ActionResult> SaveAsync(Guid id)
    {
        if (_projectManagementService.ActiveProjectId != id)
        {
            _logger.LogWarning("Project {id} is not active.", id);
            // The project is not active anymore.
            return Conflict($"Project {id} is not active.");
        }

        var result = await _projectManagementService.TrySaveActiveAsync();
        return result.IsSuccessful ? Ok() : Conflict(result.Message);
    }

}