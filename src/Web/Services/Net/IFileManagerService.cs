using static AyBorg.Web.Services.Net.FileManagerService;

namespace AyBorg.Web.Services.Net;

public interface IFileManagerService
{
    ValueTask SendImageAsync(SendImageParameters parameters);
    ValueTask ConfirmUpload(ConfirmUploadParameters parameters);
    ValueTask<ImageCollectionMeta> GetImageCollectionMetaAsync(GetImageCollectionMetaParameters parameters);
}