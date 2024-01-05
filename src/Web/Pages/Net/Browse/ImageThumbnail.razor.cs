using AyBorg.Web.Services.Net;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Net.Browse;

public partial class ImageThumbnail : ComponentBase
{
    [Parameter, EditorRequired] public string ProjectId { get; init; } = string.Empty;
    [Parameter, EditorRequired] public string ImageName { get; init; } = string.Empty;
    [Inject] IFileManagerService FileManagerService { get; init; } = null!;

    private FileManagerService.ImageContainer _imageContainer = null!;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        _imageContainer = await FileManagerService.DownloadImageAsync(new FileManagerService.DownloadImageParameters(
                ProjectId: ProjectId,
                ImageName: ImageName,
                AsThumbnail: true
                ));
    }

    private static string ToBase64String(FileManagerService.ImageContainer imageContainer)
    {
        if (imageContainer == null)
        {
            return string.Empty;
        }
        
        return $"data:{imageContainer.ContentType};base64,{imageContainer.Base64Image}";
    }
}