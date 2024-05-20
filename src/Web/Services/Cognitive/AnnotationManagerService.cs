using Ayborg.Gateway.Cognitive.V1;
using AyBorg.SDK.Common;
using AyBorg.Web.Shared.Models.Cognitive;

namespace AyBorg.Web.Services.Cognitive;

public class AnnotationManagerService : IAnnotationManagerService
{
    private readonly ILogger<AnnotationManagerService> _logger;
    private readonly AnnotationManager.AnnotationManagerClient _annotationManagerClient;

    public AnnotationManagerService(ILogger<AnnotationManagerService> logger, AnnotationManager.AnnotationManagerClient client)
    {
        _logger = logger;
        _annotationManagerClient = client;
    }

    public async ValueTask<Meta> GetMetaAsync(GetMetaParameters parameters)
    {
        AnnotationMeta response = await _annotationManagerClient.GetMetaAsync(new GetAnnotationMetaRequest
        {
            ProjectId = parameters.ProjectId,
            ImageName = parameters.ImageName
        });

        return new Meta(response.Tags, response.LayerIds);
    }

    public async ValueTask<RectangleLayer> GetRectangleLayer(GetRectangleLayerParameters parameters)
    {

        AnnotationLayer response = await _annotationManagerClient.GetAsync(new GetAnnotationRequest
        {
            ProjectId = parameters.ProjectId,
            ImageName = parameters.ImageName,
            Type = (int)parameters.ProjectType,
            LayerId = parameters.LayerId
        });

        Point p1 = response.Points[0];
        Point p2 = response.Points[1];

        return new RectangleLayer
        {
            Id = response.Id,
            ClassIndex = response.ClassIndex,
            Shape = new Shared.LabelRectangle(p1.X, p1.Y, p2.X, p2.Y)
        };
    }

    public async ValueTask AddAsync(AddParameters parameters)
    {

        AnnotationLayer layer = new()
        {
            Id = parameters.LayerId.ToString(),
            ClassIndex = parameters.ClassIndex
        };

        foreach (SDK.Common.Models.Point point in parameters.Points)
        {
            layer.Points.Add(new Point
            {
                X = point.X,
                Y = point.Y
            });
        }

        await _annotationManagerClient.AddAsync(new AddAnnotationRequest
        {
            ProjectId = parameters.ProjectId,
            ImageName = parameters.ImageName,
            Type = (int)parameters.ProjectType,
            Layer = layer
        });

        _logger.LogInformation((int)EventLogType.UserInteraction, "Added annotation to project {ProjectId}, image {ImageName} for class {ClassIndex}", parameters.ProjectId, parameters.ImageName, parameters.ClassIndex);
    }

    public async ValueTask RemoveAsync(RemoveParameters parameters)
    {

        await _annotationManagerClient.RemoveAsync(new RemoveAnnotationRequest
        {
            ProjectId = parameters.ProjectId,
            ImageName = parameters.ImageName,
            LayerId = parameters.LayerId
        });

        _logger.LogInformation((int)EventLogType.UserInteraction, "Removed annotation from project {ProjectId}, image {ImageName} for layer {LayerId}.", parameters.ProjectId, parameters.ImageName, parameters.LayerId);

    }

    public sealed record GetMetaParameters(string ProjectId, string ImageName);
    public sealed record AddParameters(string ProjectId, string ImageName, ProjectType ProjectType, string LayerId, int ClassIndex, IEnumerable<SDK.Common.Models.Point> Points);
    public sealed record RemoveParameters(string ProjectId, string ImageName, string LayerId);
    public sealed record GetRectangleLayerParameters(string ProjectId, string ImageName, ProjectType ProjectType, string LayerId);
    public sealed record Meta(IEnumerable<string> Tags, IEnumerable<string> LayerIds);
}
