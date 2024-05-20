using Ayborg.Gateway.Cognitive.V1;
using AyBorg.SDK.Common;
using Grpc.Core;

namespace AyBorg.Web.Services.Cognitive;

public class DatasetManagerService : IDatasetManagerService
{
    private readonly ILogger<DatasetManagerService> _logger;
    private readonly DatasetManager.DatasetManagerClient _datasetManagerClient;

    public DatasetManagerService(ILogger<DatasetManagerService> logger, DatasetManager.DatasetManagerClient client)
    {
        _logger = logger;
        _datasetManagerClient = client;
    }

    public async ValueTask<IEnumerable<Shared.Models.Cognitive.DatasetMeta>> GetMetasAsync(GetMetasParameters parameters)
    {
        try
        {
            AsyncServerStreamingCall<DatasetMeta> response = _datasetManagerClient.GetMetas(new GetMetasRequest
            {
                ProjectId = parameters.ProjectId
            });

            var metas = new List<Shared.Models.Cognitive.DatasetMeta>();
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

    public async ValueTask<IEnumerable<string>> GetImageNamesAsync(GetImageNamesParameters parameters)
    {
        try
        {
            AsyncServerStreamingCall<ImageInfo> response = _datasetManagerClient.GetImageNames(new GetImagesRequest
            {
                ProjectId = parameters.ProjectId,
                Id = parameters.DatasetId
            });

            var imageNames = new List<string>();
            await foreach (ImageInfo imageInfoDto in response.ResponseStream.ReadAllAsync())
            {
                imageNames.Add(imageInfoDto.Name);
            }

            return imageNames;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), ex, "Failed to get dataset image names!");
            throw;
        }
    }

    public async ValueTask<Shared.Models.Cognitive.DatasetMeta> CreateAsync(CreateParameters parameters)
    {
        try
        {
            DatasetMeta response = await _datasetManagerClient.CreateAsync(new CreateRequest
            {
                ProjectId = parameters.ProjectId,
                WithdrawDatasetImages = parameters.Withdraw
            });

            Shared.Models.Cognitive.DatasetMeta model = ToModel(response);
            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Created new dataset draft for project {ProjectId}", parameters.ProjectId);
            return model;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), ex, "Failed to create new dataset!");
            throw;
        }
    }

    public async ValueTask DeleteAsync(DeleteParameters parameters)
    {
        try
        {
            await _datasetManagerClient.DeleteAsync(new DeleteRequest
            {
                ProjectId = parameters.ProjectId,
                Id = parameters.DatasetId
            });

            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Deleted dataset from project {ProjectId} with id {DatasetId}", parameters.ProjectId, parameters.DatasetId);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), ex, "Failed to delete dataset!");
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
                Id = parameters.DatasetId,
                Options = new Ayborg.Gateway.Cognitive.V1.GenerateOptions
                {
                    SampleRate = parameters.Options.SampleRate,
                    MaxSize = parameters.Options.MaxSize,
                    FlipHorizontalProbability = parameters.Options.FlipHorizontalProbability,
                    FlipVerticalProbability = parameters.Options.FlipVerticalProbability,
                    Rotate90Probability = parameters.Options.Rotate90Probability,
                    ScaleProbability = parameters.Options.ScaleProbability,
                    PixelDropoutProbability = parameters.Options.PixelDropoutProbability,
                    ChannelShuffelProbability = parameters.Options.ChannelShuffleProbability,
                    IsoNoiseProbability = parameters.Options.IsoNoiseProbability,
                    GaussNoiseProbability = parameters.Options.GaussNoiseProbability,
                    BrightnessAndContrastProbability = parameters.Options.BrightnessAndContrastProbability
                }
            });

            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Generated new dataset for project [{ProjectId}], draft [{DatasetId}]", parameters.ProjectId, parameters.DatasetId);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), ex, "Failed to generate dataset!");
            throw;
        }
    }

    private static Shared.Models.Cognitive.DatasetMeta ToModel(DatasetMeta datasetMetaDto)
    {
        DateTime archieved_date = DateTime.MinValue;
        if (datasetMetaDto.GeneratedDate != null)
        {
            archieved_date = datasetMetaDto.GeneratedDate.ToDateTime();
        }

        var meta = new Shared.Models.Cognitive.DatasetMeta
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
    public sealed record GetImageNamesParameters(string ProjectId, string DatasetId);
    public sealed record CreateParameters(string ProjectId, bool Withdraw);
    public sealed record DeleteParameters(string ProjectId, string DatasetId);
    public sealed record AddImageParameters(string ProjectId, string DatasetId, string ImageName);
    public sealed record EditParameters(string ProjectId, Shared.Models.Cognitive.DatasetMeta Meta);
    public sealed record GenerateParameters(string ProjectId, string DatasetId, GenerateOptions Options);
    public sealed record GenerateOptions
    {
        public int MaxSize { get; init; } = 1024;
        public float FlipHorizontalProbability { get; init; } = 0f;
        public float FlipVerticalProbability { get; init; } = 0f;
        public float Rotate90Probability { get; init; } = 0f;
        public float ScaleProbability { get; init; } = 0f;
        public float PixelDropoutProbability { get; init; } = 0f;
        public float ChannelShuffleProbability { get; init; } = 0f;
        public float IsoNoiseProbability { get; init; } = 0f;
        public float GaussNoiseProbability { get; init; } = 0f;
        public float BrightnessAndContrastProbability { get; init; } = 0f;
        public int SampleRate { get; init; } = 10;
    }
}
