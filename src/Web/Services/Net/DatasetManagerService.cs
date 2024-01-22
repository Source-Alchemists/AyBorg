using Ayborg.Gateway.Net.V1;
using AyBorg.SDK.Common;
using Grpc.Core;

namespace AyBorg.Web.Services.Net;

public class DatasetManagerService : IDatasetManagerService
{
    private readonly ILogger<DatasetManagerService> _logger;
    private readonly DatasetManager.DatasetManagerClient _datasetManagerClient;

    public DatasetManagerService(ILogger<DatasetManagerService> logger, DatasetManager.DatasetManagerClient client)
    {
        _logger = logger;
        _datasetManagerClient = client;
    }

    public async ValueTask<IEnumerable<Shared.Models.Net.DatasetMeta>> GetMetasAsync(GetMetasParameters parameters)
    {
        try
        {
            AsyncServerStreamingCall<DatasetMeta> response = _datasetManagerClient.GetMetas(new GetMetasRequest
            {
                ProjectId = parameters.ProjectId
            });

            var metas = new List<Shared.Models.Net.DatasetMeta>();
            await foreach (DatasetMeta metaDto in response.ResponseStream.ReadAllAsync())
            {
                metas.Add(ToModel(metaDto));
            }

            return metas;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), ex, "Failed to get dataset metas!");
            throw;
        }
    }

    public async ValueTask<Shared.Models.Net.DatasetMeta> CreateAsync(CreateParameters parameters)
    {
        try
        {
            DatasetMeta response = await _datasetManagerClient.CreateAsync(new CreateRequest
            {
                ProjectId = parameters.ProjectId,
                WithdrawDatasetImages = parameters.Withdraw
            });

            Shared.Models.Net.DatasetMeta model = ToModel(response);
            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Created new dataset draft for porject {ProjectId}", parameters.ProjectId);
            return model;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), ex, "Failed to create new dataset!");
            throw;
        }
    }

    public async ValueTask AddImageAsync(AddImageParameters parameters)
    {
        try
        {
            await _datasetManagerClient.AddImageAsync(new AddImageRequest
            {
                ProjectId = parameters.ProjectId,
                Id = parameters.DatasetId,
                ImageName = parameters.ImageName
            });

            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Added image {ImageName} to project {ProjectId}, dataset {DatasetId}", parameters.ImageName, parameters.ProjectId, parameters.DatasetId);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), ex, "Failed to add image to dataset!");
            throw;
        }
    }

    public async ValueTask EditAsync(EditParameters parameters)
    {
        try
        {
            await _datasetManagerClient.EditAsync(new EditRequest
            {
                ProjectId = parameters.ProjectId,
                Id = parameters.Meta.Id,
                NewName = parameters.Meta.Name,
                NewComment = parameters.Meta.Comment
            });

            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Edited dataset {DatasetId} with new name [{Name}], new comment [{Comment}]", parameters.Meta.Id, parameters.Meta.Name, parameters.Meta.Comment);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), ex, "Failed to edit dataset!");
            throw;
        }
    }

    public async ValueTask GenerateAsync(GenerateParameters parameters)
    {
        try
        {
            await _datasetManagerClient.GenerateAsync(new GenerateRequest
            {
                ProjectId = parameters.ProjectId,
                Id = parameters.DatasetId
            });

            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Generated new dataset for project [{ProjectId}], draft [{DatasetId}]", parameters.ProjectId, parameters.DatasetId);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), ex, "Failed to generate dataset!");
            throw;
        }
    }

    private static Shared.Models.Net.DatasetMeta ToModel(DatasetMeta datasetMetaDto)
    {
        DateTime archieved_date = DateTime.MinValue;
        if (datasetMetaDto.GeneratedDate != null)
        {
            archieved_date = datasetMetaDto.GeneratedDate.ToDateTime();
        }

        var meta = new Shared.Models.Net.DatasetMeta
        {
            Id = datasetMetaDto.Id,
            Name = datasetMetaDto.Name,
            Comment = datasetMetaDto.Comment,
            CreationDate = datasetMetaDto.CreationDate.ToDateTime(),
            GeneratedDate = archieved_date,
            Distribution = datasetMetaDto.Distribution,
            IsActive = datasetMetaDto.IsActive
        };

        return meta;
    }

    public sealed record GetMetasParameters(string ProjectId);
    public sealed record CreateParameters(string ProjectId, bool Withdraw);
    public sealed record AddImageParameters(string ProjectId, string DatasetId, string ImageName);
    public sealed record EditParameters(string ProjectId, Shared.Models.Net.DatasetMeta Meta);
    public sealed record GenerateParameters(string ProjectId, string DatasetId);
}
