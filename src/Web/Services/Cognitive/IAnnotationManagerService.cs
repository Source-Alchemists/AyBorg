using AyBorg.Web.Shared.Models.Cognitive;
using static AyBorg.Web.Services.Cognitive.AnnotationManagerService;

namespace AyBorg.Web.Services.Cognitive;

public interface IAnnotationManagerService
{
    ValueTask<Meta> GetMetaAsync(GetMetaParameters parameters);
    ValueTask<RectangleLayer> GetRectangleLayer(GetRectangleLayerParameters parameters);
    ValueTask AddAsync(AddParameters parameters);
    ValueTask RemoveAsync(RemoveParameters parameters);
}
