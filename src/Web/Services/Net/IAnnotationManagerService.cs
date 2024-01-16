using AyBorg.Web.Shared.Models.Net;
using static AyBorg.Web.Services.Net.AnnotationManagerService;

namespace AyBorg.Web.Services.Net;

public interface IAnnotationManagerService
{
    ValueTask<Meta> GetMetaAsync(GetMetaParameters parameters);
    ValueTask<RectangleLayer> GetRectangleLayer(GetRectangleLayerParameters parameters);
    ValueTask AddAsync(AddParameters parameters);
    ValueTask RemoveAsync(RemoveParameters parameters);
}