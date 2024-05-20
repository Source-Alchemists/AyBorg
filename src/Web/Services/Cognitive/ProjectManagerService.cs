using System.Collections.Immutable;
using Ayborg.Gateway.Cognitive.V1;
using AyBorg.SDK.Common;
using AyBorg.Web.Shared.Models.Cognitive;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ProjectMeta = AyBorg.Web.Shared.Models.Cognitive.ProjectMeta;

namespace AyBorg.Web.Services.Cognitive;

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
            AsyncServerStreamingCall<Ayborg.Gateway.Cognitive.V1.ProjectMeta> response = _projectManagerClient.GetMetas(new Empty());
            await foreach (Ayborg.Gateway.Cognitive.V1.ProjectMeta? metaDto in response.ResponseStream.ReadAllAsync())
            {
                metas.Add(ToModel(metaDto));
            }

            return metas;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), ex, "Failed to get project metas!");
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
            Ayborg.Gateway.Cognitive.V1.ProjectMeta response = await _projectManagerClient.CreateAsync(request);

            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Project created: {ProjectName}", response.Name);
            return ToModel(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectSaved), ex, "Failed to create project!");
            throw;
        }
    }

    public async ValueTask DeleteAsync(DeleteRequestParameters parameters)
    {
        try
        {
            await _projectManagerClient.DeleteAsync(new Ayborg.Gateway.Cognitive.V1.ProjectMeta
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

    public async ValueTask<Shared.Models.Cognitive.ClassLabel> AddOrUpdateAsync(AddOrUpdateClassLabelParameters parameters)
    {
        try
        {
            Ayborg.Gateway.Cognitive.V1.ClassLabel response = await _projectManagerClient.AddOrUpdateClassLabelAsync(new AddOrUpdateClassLabelRequest
            {
                ProjectId = parameters.ProjectId,
                ClassLabel = new Ayborg.Gateway.Cognitive.V1.ClassLabel
                {
                    Index = parameters.ClassLabel.Index,
                    Name = parameters.ClassLabel.Name,
                    ColorCode = parameters.ClassLabel.ColorCode
                }
            });

            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Add or update class label: {ClassLabel} to project {ProjectId}", parameters.ClassLabel, parameters.ProjectId);

            return new Shared.Models.Cognitive.ClassLabel
            {
                Index = response.Index,
                Name = response.Name,
                ColorCode = response.ColorCode
            };
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), ex, "Failed to add or update class {ClassLabel}!", parameters.ClassLabel);
            throw;
        }
    }

    private static ProjectMeta ToModel(Ayborg.Gateway.Cognitive.V1.ProjectMeta projectMetaDto)
    {
        ImmutableList<string> tags = ImmutableList<string>.Empty;
        foreach (string tag in projectMetaDto.Tags)
        {
            tags = tags.Add(tag);
        }

        ImmutableList<Shared.Models.Cognitive.ClassLabel> classes = ImmutableList<Shared.Models.Cognitive.ClassLabel>.Empty;
        foreach (Ayborg.Gateway.Cognitive.V1.ClassLabel? classDto in projectMetaDto.ClassLabels)
        {
            classes = classes.Add(new Shared.Models.Cognitive.ClassLabel
            {
                Index = classDto.Index,
                Name = classDto.Name,
                ColorCode = classDto.ColorCode
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
    public record AddOrUpdateClassLabelParameters(string ProjectId, Shared.Models.Cognitive.ClassLabel ClassLabel);
}
