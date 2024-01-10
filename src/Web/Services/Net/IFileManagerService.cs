using static AyBorg.Web.Services.Net.FileManagerService;

namespace AyBorg.Web.Services.Net;

public interface IFileManagerService
{
    ValueTask UploadImageAsync(UploadImageParameters parameters);
    ValueTask ConfirmUploadAsync(ConfirmUploadParameters parameters);
    ValueTask<ImageContainer> DownloadImageAsync(DownloadImageParameters parameters);
    ValueTask<ImageCollectionMeta> GetImageCollectionMetaAsync(GetImageCollectionMetaParameters parameters);
    ValueTask<ImageAnnotationMeta> GetImageAnnotationMetaAsync(GetImageAnnotationMetaParameters parameters);
}