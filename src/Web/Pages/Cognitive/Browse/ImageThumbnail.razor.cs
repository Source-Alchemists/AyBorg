using AyBorg.Web.Services.Cognitive;
using Grpc.Core;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Cognitive.Browse;

public partial class ImageThumbnail : ComponentBase
{
    [Parameter, EditorRequired] public string ProjectId { get; init; } = string.Empty;
    [Parameter, EditorRequired] public string ImageName { get; init; } = string.Empty;
    [Parameter, EditorRequired] public bool Selected { get; set; } = false;
    [Parameter] public bool DatasetUsed { get; init; } = false;
    [Parameter] public EventCallback<SelectChangedArgs> OnSelectChanged { get; set; }
    [Parameter] public EventCallback<string> OnAnnotateClicked { get; set; }
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

        try
        {
            _imageContainer = await FileManagerService.DownloadImageAsync(new FileManagerService.DownloadImageParameters(
                    ProjectId: ProjectId,
                    ImageName: ImageName,
                    AsThumbnail: true
                    ));
        }
        catch (RpcException)
        {
            // Already logged.
        }
    }

    private async Task SelectedChanged(bool value)
    {
        if (value != Selected)
        {
            Selected = value;
            await OnSelectChanged.InvokeAsync(new SelectChangedArgs(ImageName, value));
        }
    }

    private async Task AnnotateClicked()
    {
        await OnAnnotateClicked.InvokeAsync(ImageName);
    }

    public sealed record SelectChangedArgs(string ImageName, bool Value);
}
