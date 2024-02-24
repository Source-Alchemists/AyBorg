using AyBorg.Web.Shared.Models;
using AyBorg.Web.Shared.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace AyBorg.Web.Shared;

public partial class FileDropArea : ComponentBase, IAsyncDisposable
{
    [Parameter] public RenderFragment ChildContent { get; init; } = null!;
    [Parameter] public string Hint { get; init; } = "Drag and drop images to upload them.";
    [Parameter] public EventCallback<ImageSource> OnImageAdded { get; init; }

    [Inject] IJSRuntime JSRuntime { get; init; } = null!;

    private InputFile _inputFileRef = null!;
    private IJSObjectReference _fileDropModule = null!;
    private IJSObjectReference _fileDropFunctionReference = null!;
    private ElementReference _fileDropContainerRef;
    private bool _isLoading = false;
    private string _dragHoverClass = "mud-border-primary mud-elevation-1";
    private bool _isDisposed = false;

    protected override async Task OnAfterRenderAsync(bool firstRender) {
         await base.OnAfterRenderAsync(firstRender);

         if (firstRender)
         {
            _fileDropModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/fileDropZone.js");
            _fileDropFunctionReference = await _fileDropModule.InvokeAsync<IJSObjectReference>("initializeFileDropZone", _fileDropContainerRef, _inputFileRef.Element);
         }
    }

    private void OnDragHighlight(DragEventArgs dragEvent)
    {
        _dragHoverClass = "mud-border-info mud-elevation-3";
    }

    private void OnDragUnhighlight(DragEventArgs dragEvent)
    {
        _dragHoverClass = "mud-border-primary mud-elevation-1";
    }

    private void OnDrop(DragEventArgs dragEvent)
    {
        _dragHoverClass = "mud-border-primary mud-elevation-1";
    }

    private async Task OnInputFileChange(InputFileChangeEventArgs eventArgs)
    {
        _isLoading = true;
        await InvokeAsync(StateHasChanged);

        try
        {
            foreach (IBrowserFile file in eventArgs.GetMultipleFiles(maximumFileCount: int.MaxValue).Where(f => f.ContentType.StartsWith("image/")))
            {
                await AddImageSourceAsync(file);
            }
        }
        finally
        {
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void OnUploadFilesChanged(IReadOnlyList<IBrowserFile> files)
    {
        _isLoading = true;
        await InvokeAsync(StateHasChanged);

        try
        {
            foreach (IBrowserFile file in files)
            {
                await AddImageSourceAsync(file);
            }
        }
        finally
        {
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async ValueTask AddImageSourceAsync(IBrowserFile file)
    {
        ImageSource imageSource = await UploadUtils.CreateImageSourceAsync(file);
        await OnImageAdded.InvokeAsync(imageSource);
    }

    public async ValueTask DisposeAsync()
    {

        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }

    private async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            try
            {
                if (_fileDropFunctionReference != null)
                {
                    await _fileDropFunctionReference.InvokeVoidAsync("dispose");
                    await _fileDropFunctionReference.DisposeAsync();
                }

                if (_fileDropModule != null)
                {
                    await _fileDropModule.DisposeAsync();
                }
            }
            catch (JSDisconnectedException)
            {
                // No need to log, can only happen on site reload as SignalR is already disconnected.
                // There is no memory leak in this case.
            }
            _isDisposed = true;
        }
    }
}
