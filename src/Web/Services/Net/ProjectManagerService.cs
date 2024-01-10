using System.Collections.Immutable;
using Ayborg.Gateway.Net.V1;
using AyBorg.SDK.Common;
using AyBorg.Web.Shared.Models.Net;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ProjectMeta = AyBorg.Web.Shared.Models.Net.ProjectMeta;

namespace AyBorg.Web.Services.Net;

public class ProjectManagerService : IProjectManagerService
{
    private readonly ILogger<ProjectManagerService> _logger;
    private readonly ProjectManager.ProjectManagerClient _projectManagerClient;

    public ProjectManagerService(ILogger<ProjectManagerService> logger, ProjectManager.ProjectManagerClient projectManagerClient)
    {
        _logger = logger;
        _projectManagerClient = projectManagerClient;
    }

    public async ValueTask<IEnumerable<ProjectMeta>> GetMetasAsync()
    {
        try
        {
            var metas = new List<ProjectMeta>();
            AsyncServerStreamingCall<Ayborg.Gateway.Net.V1.ProjectMeta> response = _projectManagerClient.GetMetas(new Empty());
            await foreach (Ayborg.Gateway.Net.V1.ProjectMeta? metaDto in response.ResponseStream.ReadAllAsync())
            {
                metas.Add(ToModel(metaDto));
            }

            return metas;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Connect), ex, "Failed to connect");
            throw;
        }
    }

    public async ValueTask<ProjectMeta> CreateAsync(CreateRequestParameters parameters)
    {
        var request = new CreateProjectRequest
        {
            Name = parameters.Name,
            Type = (int)parameters.Type,
            CreatedBy = parameters.Creator,
        };

        foreach (string tag in parameters.Tags)
        {
            request.Tags.Add(tag);
        }

        try
        {
            Ayborg.Gateway.Net.V1.ProjectMeta response = await _projectManagerClient.CreateAsync(request);

            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Project created: {ProjectName}", response.Name);
            return ToModel(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectSaved), ex, "Failed to create project");
            throw;
        }
    }

    public async ValueTask DeleteAsync(DeleteRequestParameters parameters)
    {
        try
        {
            await _projectManagerClient.DeleteAsync(new Ayborg.Gateway.Net.V1.ProjectMeta
            {
                Id = parameters.ProjectId,
                CreatedBy = parameters.Username
            });

            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Project deleted: {ProjectId}", parameters.ProjectId);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectRemoved), ex, "Failed to delete project");
            throw;
        }
    }

    private static ProjectMeta ToModel(Ayborg.Gateway.Net.V1.ProjectMeta projectMetaDto)
    {
        ImmutableList<string> tags = ImmutableList<string>.Empty;
        foreach (string tag in projectMetaDto.Tags)
        {
            tags = tags.Add(tag);
        }

        ImmutableList<ClassLabel> classes = ImmutableList<Shared.Models.Net.ClassLabel>.Empty;
        foreach (Label? LabelDto in projectMetaDto.Labels)
        {
            classes = classes.Add(new Shared.Models.Net.ClassLabel
            {
                Name = LabelDto.Name,
                ColorCode = LabelDto.ColorCode
            });
        }

        return new ProjectMeta
        {
            Id = projectMetaDto.Id,
            Name = projectMetaDto.Name,
            Type = (ProjectType)projectMetaDto.Type,
            Creator = projectMetaDto.CreatedBy,
            Created = projectMetaDto.CreationDate.ToDateTime(),
            Tags = tags,
            Classes = classes
        };
    }

    public record CreateRequestParameters(string Name, ProjectType Type, string Creator, IEnumerable<string> Tags);
    public record DeleteRequestParameters(string ProjectId, string Username);
}