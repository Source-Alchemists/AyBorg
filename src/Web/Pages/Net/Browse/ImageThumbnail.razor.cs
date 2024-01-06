using AyBorg.Web.Services.Net;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Net.Browse;

public partial class ImageThumbnail : ComponentBase
{
    [Parameter, EditorRequired] public string ProjectId { get; init; } = string.Empty;
    [Parameter, EditorRequired] public string ImageName { get; init; } = string.Empty;
    [Parameter, EditorRequired] public bool Selected { get; set; } = false;
    [Parameter] public EventCallback<SelectChangedArgs> OnSelectChanged { get; set; }
    [Inject] IFileManagerService FileManagerService { get; init; } = null!;

    private FileManagerService.ImageContainer _imageContainer = null!;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (_imageContainer != null && _imageContainer.ImageName.Equals(ImageName, StringComparison.InvariantCultureIgnoreCase))
        {
            // Nothing to do, as we already received the image.
            return;
        }

        _imageContainer = await FileManagerService.DownloadImageAsync(new FileManagerService.DownloadImageParameters(
                ProjectId: ProjectId,
                ImageName: ImageName,
                AsThumbnail: true
                ));
    }

    private async void OnSelectedChanged(bool value)
    {
        if (value != Selected)
        {
            Selected = value;
            await OnSelectChanged.InvokeAsync(new SelectChangedArgs(ImageName, value));
        }
    }

    private static string ToBase64String(FileManagerService.ImageContainer imageContainer)
    {
        if (imageContainer == null)
        {
            return string.Empty;
        }

        return $"data:{imageContainer.ContentType};base64,{imageContainer.Base64Image}";
    }

    public sealed record SelectChangedArgs(string ImageName, bool Value);
}