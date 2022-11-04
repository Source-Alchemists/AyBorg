using Microsoft.AspNetCore.Mvc;
using Atomy.Agent.Services;
using Atomy.SDK.Data.DTOs;
using Atomy.SDK.Data.Mapper;
using Atomy.SDK.Authorization;
using Atomy.SDK.Projects;

namespace Atomy.Agent.Controllers;

[ApiController]
[Route("[controller]")]
[JwtAuthorize(Roles = new[] { Roles.Administrator, Roles.Engineer })]
public class ProjectsController : ControllerBase
{
    private readonly ILogger<ProjectsController> _logger;
    private readonly IProjectManagementService _projectManagementService;
    private readonly IDtoMapper _storageToDtoMapper;
    private readonly string _serviceUniqueName;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectsController"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="projectManagementService">The project management service.</param>
    /// <param name="storageToDtoMapper">The storage to dto mapper.</param>
    public ProjectsController(ILogger<ProjectsController> logger, IConfiguration configuration, IProjectManagementService projectManagementService, IDtoMapper storageToDtoMapper)
    {
        _logger = logger;
        _projectManagementService = projectManagementService;
        _storageToDtoMapper = storageToDtoMapper;
        _serviceUniqueName = configuration.GetValue<string>("Atomy:Service:UniqueName");
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async IAsyncEnumerable<ProjectMetaDto> GetAsync()
    {
        foreach (var meta in (await _projectManagementService.GetAllMetasAsync()).Where(x => x.ServiceUniqueName == _serviceUniqueName))
        {
            var projectMetaDto = _storageToDtoMapper.Map(meta);
            yield return projectMetaDto;
        }
    }

    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectMetaDto>> GetActiveAsync()
    {
        var metas = await _projectManagementService.GetAllMetasAsync();
        var activeProjectMeta = metas.FirstOrDefault(p => p.IsActive && p.ServiceUniqueName == _serviceUniqueName);
        if(activeProjectMeta == null)
        {
            _logger.LogWarning("No active project found.");
            return NotFound();
        }

        var projectMetaDto = _storageToDtoMapper.Map(activeProjectMeta);
        return Ok(projectMetaDto);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ProjectMetaDto>> CreateAsync(string name)
    {
        var record = await _projectManagementService.CreateAsync(name);
        return Ok(_storageToDtoMapper.Map(record.Meta));
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAsync(Guid id)
    {
        return await _projectManagementService.TryDeleteAsync(id) ? Ok() : NotFound();
    }

    [HttpPut("{id}/active/{isActive}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ActivateAsync(Guid id, bool isActive)
    {
        return await _projectManagementService.TryActivateAsync(id, isActive) ? Ok() : NotFound();
    }

    [HttpPut("{id}/state/{state}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> ChangeStateAsync(Guid id, ProjectState state)
    {
        return await _projectManagementService.TryChangeProjectStateAsync(id, state) ? Ok() : Conflict();
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> SaveAsync(Guid id)
    {
        if(_projectManagementService.ActiveProjectId != id)
        {
            _logger.LogWarning("Project {id} is not active.", id);
            // The project is not active anymore.
            return Conflict();
        }

        return await _projectManagementService.TrySaveActiveProjectAsync() ? Ok() : Conflict();
    }

}