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

    public async ValueTask<ProjectMeta> CreateAsync(CreateRequestOptions options)
    {
        var request = new CreateProjectRequest
        {
            Name = options.Name,
            Type = (int)options.Type,
            CreatedBy = options.Creator,
        };

        foreach (Shared.Models.Net.Tag tag in options.Tags)
        {
            request.Tags.Add(new Ayborg.Gateway.Net.V1.Tag
            {
                Name = tag.Name,
                ColorCode = tag.ColorCode
            });
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

    public async ValueTask DeleteAsync(DeleteRequestOptions options)
    {
        try
        {
            await _projectManagerClient.DeleteAsync(new Ayborg.Gateway.Net.V1.ProjectMeta
            {
                Id = options.ProjectId,
                CreatedBy = options.Username
            });

            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Project deleted: {ProjectId}", options.ProjectId);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectRemoved), ex, "Failed to delete project");
            throw;
        }
    }

    private static ProjectMeta ToModel(Ayborg.Gateway.Net.V1.ProjectMeta projectMetaDto)
    {
        ImmutableList<Shared.Models.Net.Tag> tags = ImmutableList<Shared.Models.Net.Tag>.Empty;

        foreach (Ayborg.Gateway.Net.V1.Tag? tagDto in projectMetaDto.Tags)
        {
            tags = tags.Add(new Shared.Models.Net.Tag
            {
                Name = tagDto.Name,
                ColorCode = tagDto.ColorCode
            });
        }

        return new ProjectMeta
        {
            Id = projectMetaDto.Id,
            Name = projectMetaDto.Name,
            Type = (ProjectType)projectMetaDto.Type,
            Creator = projectMetaDto.CreatedBy,
            Created = projectMetaDto.CreationDate.ToDateTime(),
            Tags = tags
        };
    }

    public record CreateRequestOptions(string Name, ProjectType Type, string Creator, IEnumerable<Shared.Models.Net.Tag> Tags);
    public record DeleteRequestOptions(string ProjectId, string Username);
}