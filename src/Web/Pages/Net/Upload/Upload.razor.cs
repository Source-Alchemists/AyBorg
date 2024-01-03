using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Net;
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.IO;
using Microsoft.JSInterop;
using MudBlazor;

namespace AyBorg.Web.Pages.Net.Upload;

public partial class Upload : ComponentBase, IAsyncDisposable
{
    [Parameter] public string ProjectId { get; init; } = string.Empty;
    [Inject] IStateService StateService { get; init; } = null!;
    [Inject] IProjectManagerService ProjectManagerService { get; init; } = null!;
    [Inject] IFileManagerService FileManagerService { get; init; } = null!;
    [Inject] IDialogService DialogService { get; init; } = null!;
    [Inject] ISnackbar Snackbar { get; init; } = null!;
    [Inject] NavigationManager NavigationManager { get; init; } = null!;
    [Inject] IJSRuntime JSRuntime { get; init; } = null!;

    private const int MAX_FILE_SIZE = 51200000; // 50MB
    private static readonly RecyclableMemoryStreamManager s_memoryManager = new();
    private string _projectName = string.Empty;
    private string _dragHoverClass = "mud-border-primary mud-elevation-1";
    private ElementReference _fileDropContainerRef;
    private InputFile _inputFileRef = null!;
    private MudAutocomplete<string> _tagField = null!;
    private IJSObjectReference _fileDropModule = null!;
    private IJSObjectReference _fileDropFunctionReference = null!;
    private HashSet<ImageSource> _imageSources = new();
    private IEnumerable<string> _projectTags = new List<string>();
    private IEnumerable<string> _availableTags => _projectTags.Where(t => !_selectedTags.Contains(t));
    private ImmutableList<string> _selectedTags = ImmutableList<string>.Empty;
    private bool _isLoading = true;
    private bool _isDisposed = false;
    private string _batchPlaceholder = string.Empty;
    private string _batchName = string.Empty;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            _fileDropModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/fileDropZone.js");
            _fileDropFunctionReference = await _fileDropModule.InvokeAsync<IJSObjectReference>("initializeFileDropZone", _fileDropContainerRef, _inputFileRef.Element);

            await StateService.UpdateStateFromSessionStorageAsync();
            IEnumerable<Shared.Models.Net.ProjectMeta> metas = await ProjectManagerService.GetMetasAsync();
            Shared.Models.Net.ProjectMeta? targetMeta = metas.FirstOrDefault(m => m.Id.Equals(ProjectId, StringComparison.InvariantCultureIgnoreCase));
            if (StateService.NetState != null)
            {
                _projectName = StateService.NetState.ProjectName;
            }
            else
            {
                if (targetMeta != null)
                {
                    _projectName = targetMeta.Name;
                    await StateService.SetNetStateAsync(new Shared.Models.UiNetState(targetMeta));
                }
            }

            if (targetMeta != null)
            {
                _projectTags = targetMeta.Tags;
            }

            _batchPlaceholder = $"Uploaded on {DateTime.UtcNow}";

            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task<IEnumerable<string>> SearchTags(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return _availableTags;
        }

        return await Task.FromResult(
            _availableTags.Where(
                t => t.Contains(value, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(t => t)
            );
    }

    private async void OnTagsKeyUp(KeyboardEventArgs args)
    {
        if (args.Code.Equals("Space", StringComparison.InvariantCultureIgnoreCase)
            || args.Code.Equals("Enter", StringComparison.InvariantCultureIgnoreCase)
            || args.Code.Equals("NumpadEnter", StringComparison.InvariantCultureIgnoreCase)
            || args.Code.Equals("Tab", StringComparison.InvariantCultureIgnoreCase))
        {
            AddTag(_tagField.Text);
            await _tagField.BlurAsync();
            await _tagField.Clear();
            await _tagField.FocusAsync();
        }
    }

    private void OnTagsValueChanged(string value)
    {
        AddTag(value);
        _tagField.Clear();
    }

    private void TagRemoved(MudChip chip)
    {
        _selectedTags = _selectedTags.Remove(chip.Text);
    }

    private void AddTag(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        if (_selectedTags.Contains(value))
        {
            return;
        }

        _selectedTags = _selectedTags.Add(value);
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
        _isLoading = true;
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
        }

        await InvokeAsync(StateHasChanged);
    }


    private async Task OnInputFileChange(InputFileChangeEventArgs eventArgs)
    {
        _isLoading = true;
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
        }
    }

    private async Task OnSave()
    {

        IDialogReference dialogReference = DialogService.Show<ImagesDistributionDialog>("How should the images be organised?", new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        });

        DialogResult result = await dialogReference.Result;
        if (result.Canceled)
        {
            return;
        }

        _isLoading = true;
        await InvokeAsync(StateHasChanged);
        ImagesDistributionDialog.Result distribution = (ImagesDistributionDialog.Result)result.Data;

        if (string.IsNullOrEmpty(_batchName))
        {
            _batchName = _batchPlaceholder;
        }

        try
        {
            int imageIndex = 0;
            int imageCount = _imageSources.Count;
            string collectionId = Guid.NewGuid().ToString();
            foreach (ImageSource image in _imageSources)
            {
                await FileManagerService.SendImageAsync(
                    new FileManagerService.SendImageParameters(
                        ProjectId,
                        image.Data,
                        image.ContentType,
                        collectionId,
                        imageIndex,
                        imageCount)
                    );
                imageIndex++;
            }

            await FileManagerService.ConfirmUpload(new FileManagerService.ConfirmUploadParameters(
                ProjectId,
                collectionId,
                _batchName,
                _selectedTags,
                new int[] { distribution.TrainFactor, distribution.ValidFactor, distribution.TestFactor }));

            Snackbar.Add("Upload finished!", Severity.Info);
            NavigationManager.NavigateTo($"net/annotate/{ProjectId}");
        }
        catch (RpcException)
        {
            Snackbar.Add("Failed to upload images!", Severity.Warning);
        }
        finally
        {
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async ValueTask AddImageSourceAsync(IBrowserFile file)
    {
        using Stream stream = file.OpenReadStream(maxAllowedSize: MAX_FILE_SIZE);
        using MemoryStream ms = s_memoryManager.GetStream();
        await stream.CopyToAsync(ms);
        byte[] data = ms.ToArray();
        byte[] hashBytes = SHA256.HashData(data);
        var stringBuilder = new StringBuilder();
        for (int index = 0; index < hashBytes.Length; index++)
        {
            stringBuilder.Append(hashBytes[index].ToString("x2"));
        }

        string hashValue = stringBuilder.ToString();
        if (_imageSources.Any(s => s.Hash.Equals(hashValue, StringComparison.InvariantCultureIgnoreCase)))
        {
            return;
        }

        _imageSources.Add(new ImageSource(data, file.ContentType, hashValue));
    }

    private async void OnDeleteImageClicked(string hashValue)
    {
        ImageSource? targetImage = _imageSources.FirstOrDefault(s => s.Hash.Equals(hashValue, StringComparison.InvariantCultureIgnoreCase));
        if (targetImage != null)
        {
            _imageSources.Remove(targetImage);
            await InvokeAsync(StateHasChanged);
        }
    }

    private static string ToBase64String(ImageSource imageSource)
    {
        return $"data:{imageSource.ContentType};base64,{Convert.ToBase64String(imageSource.Data)}";
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

    private sealed record ImageSource(byte[] Data, string ContentType, string Hash);
}