using static AyBorg.Web.Services.Net.FileManagerService;

namespace AyBorg.Web.Services.Net;

public interface IFileManagerService
{
    ValueTask UploadImageAsync(UploadImageParameters parameters);
    ValueTask ConfirmUploadAsync(ConfirmUploadParameters parameters);
    ValueTask<ImageContainer> DownloadImageAsync(DownloadImageParameters parameters);
    ValueTask<ImageCollectionMeta> GetImageCollectionMetaAsync(GetImageCollectionMetaParameters parameters);
    ValueTask<IEnumerable<ModelMeta>> GetModelMetasAsync(GetModelMetasParameters parameters);
    ValueTask EditModelAsync(EditModelParameters parameters);
    ValueTask DeleteModelAsync(DeleteModelParameters parameters);
}
