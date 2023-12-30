using AyBorg.Web.Services;
using AyBorg.Web.Services.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace AyBorg.Web.Pages.Net.Upload;

public partial class Upload : ComponentBase, IAsyncDisposable
{
    [Parameter] public string ProjectId { get; init; } = string.Empty;
    [Inject] IStateService StateService { get; init; } = null!;
    [Inject] IProjectManagerService ProjectManagerService { get; init; } = null!;
    [Inject] IJSRuntime JSRuntime { get; init; } = null!;

    private const int MAX_FILE_SIZE = 51200000; // 50MB
    private string _projectName = string.Empty;
    private string _dragHoverClass = "mud-border-primary mud-elevation-1";
    private ElementReference _fileDropContainerRef;
    private InputFile _inputFileRef = null!;
    private IJSObjectReference _fileDropModule = null!;
    private IJSObjectReference _fileDropFunctionReference = null!;
    private HashSet<string> _imageSources = new();
    private bool _isDisposed = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            _fileDropModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/fileDropZone.js");
            _fileDropFunctionReference = await _fileDropModule.InvokeAsync<IJSObjectReference>("initializeFileDropZone", _fileDropContainerRef, _inputFileRef.Element);

            await StateService.UpdateStateFromSessionStorageAsync();
            if (StateService.NetState != null)
            {
                _projectName = StateService.NetState.ProjectName;
            }
            else
            {
                IEnumerable<Shared.Models.Net.ProjectMeta> metas = await ProjectManagerService.GetMetasAsync();
                Shared.Models.Net.ProjectMeta? targetMeta = metas.FirstOrDefault(m => m.Id.Equals(ProjectId, StringComparison.InvariantCultureIgnoreCase));
                if (targetMeta != null)
                {
                    _projectName = targetMeta.Name;
                    await StateService.SetNetStateAsync(new Shared.Models.UiNetState(targetMeta));
                }
            }

            await InvokeAsync(StateHasChanged);
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

    private async void OnUploadFilesChanged(IReadOnlyList<IBrowserFile> files)
    {
        foreach (IBrowserFile file in files)
        {
            await AddBase64ImageSourceAsync(file);
        }

        await InvokeAsync(StateHasChanged);
    }


    private async Task OnInputFileChange(InputFileChangeEventArgs eventArgs)
    {
        foreach (IBrowserFile file in eventArgs.GetMultipleFiles(maximumFileCount: int.MaxValue).Where(f => f.ContentType.StartsWith("image/")))
        {
            await AddBase64ImageSourceAsync(file);
        }
    }

    private async ValueTask AddBase64ImageSourceAsync(IBrowserFile file)
    {
        using Stream stream = file.OpenReadStream(maxAllowedSize: MAX_FILE_SIZE);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        string imageSource = $"data:{file.ContentType};base64,{Convert.ToBase64String(ms.ToArray())}";
        _imageSources.Add(imageSource);
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