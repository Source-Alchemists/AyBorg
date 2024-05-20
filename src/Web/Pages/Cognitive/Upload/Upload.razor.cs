using System.Collections.Immutable;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Cognitive;
using AyBorg.Web.Shared.Models;
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace AyBorg.Web.Pages.Cognitive.Upload;

public partial class Upload : ComponentBase
{
    [Parameter] public string ProjectId { get; init; } = string.Empty;
    [Inject] IStateService StateService { get; init; } = null!;
    [Inject] IProjectManagerService ProjectManagerService { get; init; } = null!;
    [Inject] IFileManagerService FileManagerService { get; init; } = null!;
    [Inject] IDialogService DialogService { get; init; } = null!;
    [Inject] ISnackbar Snackbar { get; init; } = null!;
    [Inject] NavigationManager NavigationManager { get; init; } = null!;

    private string _projectName = string.Empty;
    private MudAutocomplete<string> _tagField = null!;
    private HashSet<ImageSource> _imageSources = new();
    private IEnumerable<string> _projectTags = new List<string>();
    private IEnumerable<string> _availableTags => _projectTags.Where(t => !_selectedTags.Contains(t)).OrderBy(t => t);
    private ImmutableList<string> _selectedTags = ImmutableList<string>.Empty;
    private bool _isLoading = true;
    private string _batchPlaceholder = string.Empty;
    private string _batchName = string.Empty;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await StateService.UpdateStateFromSessionStorageAsync();
            IEnumerable<Web.Shared.Models.Cognitive.ProjectMeta> metas = await ProjectManagerService.GetMetasAsync();
            Web.Shared.Models.Cognitive.ProjectMeta? targetMeta = metas.FirstOrDefault(m => m.Id.Equals(ProjectId, StringComparison.InvariantCultureIgnoreCase));
            if (StateService.CognitiveState != null)
            {
                _projectName = StateService.CognitiveState.ProjectName;
            }
            else
            {
                if (targetMeta != null)
                {
                    _projectName = targetMeta.Name;
                    await StateService.SetNetStateAsync(new Web.Shared.Models.UiCognitiveState(targetMeta));
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

    private async Task ImageAdded(ImageSource image)
    {
        if(_imageSources.Any(i => i.Hash.Equals(image.Hash)))
        {
            return;
        }

        _imageSources.Add(image);
        await InvokeAsync(StateHasChanged);
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
                await FileManagerService.UploadImageAsync(
                    new FileManagerService.UploadImageParameters(
                        ProjectId,
                        image.Data,
                        image.ContentType,
                        collectionId,
                        imageIndex,
                        imageCount)
                    );
                imageIndex++;
            }

            await FileManagerService.ConfirmUploadAsync(new FileManagerService.ConfirmUploadParameters(
                ProjectId,
                collectionId,
                _batchName,
                _selectedTags,
                new int[] { distribution.TrainFactor, distribution.ValidFactor, distribution.TestFactor }));

            Snackbar.Add("Upload finished!", Severity.Info);
            NavigationManager.NavigateTo($"cognitive/browse/{ProjectId}");
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

    private async void OnDeleteImageClicked(string hashValue)
    {
        ImageSource? targetImage = _imageSources.FirstOrDefault(s => s.Hash.Equals(hashValue, StringComparison.InvariantCultureIgnoreCase));
        if (targetImage != null)
        {
            _imageSources.Remove(targetImage);
            await InvokeAsync(StateHasChanged);
        }
    }
}
