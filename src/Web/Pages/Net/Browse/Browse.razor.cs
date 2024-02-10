using AyBorg.Web.Services;
using AyBorg.Web.Services.Net;
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Net.Browse;

public partial class Browse : ComponentBase
{
    [Parameter] public string ProjectId { get; init; } = string.Empty;
    [Inject] IStateService StateService { get; init; } = null!;
    [Inject] IProjectManagerService ProjectManagerService { get; init; } = null!;
    [Inject] IFileManagerService FileManagerService { get; init; } = null!;
    [Inject] IDatasetManagerService DatasetManagerService { get; init; } = null!;
    [Inject] ISnackbar Snackbar { get; init; } = null!;
    [Inject] NavigationManager NavigationManager { get; init; } = null!;

    private readonly List<string> _selectedImageNames = new();
    private readonly List<string> _projectTags = new();
    private string _projectName = string.Empty;
    private bool _isLoading = true;
    private FileManagerService.ImageCollectionMeta _imageCollectionMeta = new(Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
    private List<string> _allImageNames = new();
    private IEnumerable<string> _datasetUsedImageNames = Array.Empty<string>();
    private IEnumerable<string> _allBatchNames = Array.Empty<string>();
    private SplitGroup _selectedGroup = SplitGroup.All;
    private string _selectedGroupName = "ALL";
    private string _selectedBatchName = string.Empty;
    private IEnumerable<string> _selectedTags = Array.Empty<string>();
    private int _activePanelIndex = 0;
    private bool _isAnnotateVisible => _activePanelIndex == 0;
    private bool _isAddToDatasetVisible => _activePanelIndex == 1;
    private bool _isEditButtonEnabled => _selectedImageNames.Any();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            try
            {
                await StateService.UpdateStateFromSessionStorageAsync();
                IEnumerable<Web.Shared.Models.Net.ProjectMeta> metas = await ProjectManagerService.GetMetasAsync();
                Web.Shared.Models.Net.ProjectMeta? targetMeta = metas.FirstOrDefault(m => m.Id.Equals(ProjectId, StringComparison.InvariantCultureIgnoreCase));

                if (StateService.NetState != null)
                {
                    _projectName = StateService.NetState.ProjectName;
                }
                else
                {
                    if (targetMeta != null)
                    {
                        _projectName = targetMeta.Name;
                        await StateService.SetNetStateAsync(new Web.Shared.Models.UiNetState(targetMeta));
                    }
                }

                if (targetMeta != null)
                {
                    _projectTags.Clear();
                    _projectTags.AddRange(targetMeta.Tags.OrderBy(t => t));
                }

                await UpdateImageCollectionMeta(string.Empty, string.Empty, Array.Empty<string>());
                _allBatchNames = _imageCollectionMeta.BatchNames.OrderBy(b => b);
            }
            catch (RpcException)
            {
                Snackbar.Add("Failed to get project informations!", Severity.Warning);
            }
            finally
            {
                _isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private async Task UpdateImageCollectionMeta(string batchName, string splitGroup, IEnumerable<string> tags)
    {
        _imageCollectionMeta = await FileManagerService.GetImageCollectionMetaAsync(new FileManagerService.GetImageCollectionMetaParameters(
                            ProjectId,
                            batchName,
                            splitGroup,
                            tags));

        _datasetUsedImageNames = await DatasetManagerService.GetImageNamesAsync(new DatasetManagerService.GetImageNamesParameters(ProjectId, string.Empty));
        _selectedImageNames.Clear();
        _allImageNames.Clear();
        _allImageNames.AddRange(_imageCollectionMeta.UnannotatedFileNames);
        _allImageNames.AddRange(_imageCollectionMeta.AnnotatedFileNames);
    }

    private async Task<IEnumerable<string>> SearchBatch(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return _allBatchNames;
        }

        return await Task.FromResult(
            _allBatchNames.Where(b => b.Contains(value, StringComparison.InvariantCultureIgnoreCase))
            .OrderBy(b => b)
        );
    }

    private async Task BatchValueChanged(string value)
    {
        _isLoading = true;
        _selectedBatchName = value;
        await FetchImageCollectionMeta();
    }

    private async Task SplitGroupChanged(SplitGroup value)
    {
        switch (value)
        {
            case SplitGroup.All:
                _selectedGroupName = "ALL";
                break;
            case SplitGroup.Train:
                _selectedGroupName = "TRAIN";
                break;
            case SplitGroup.Valid:
                _selectedGroupName = "VALID";
                break;
            case SplitGroup.Test:
                _selectedGroupName = "TEST";
                break;
        }

        _isLoading = true;
        _selectedGroup = value;
        await FetchImageCollectionMeta();
    }

    private async Task SelectedTagsChanged(IEnumerable<string> values)
    {
        _isLoading = true;
        _selectedTags = values;
        await FetchImageCollectionMeta();
    }

    private async Task ThumbnailSelectionChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    private void ActivePanaelIndexChanged(int value)
    {
        _activePanelIndex = value;
        _selectedImageNames.Clear();
    }

    private void DeselectAllClicked()
    {
        _selectedImageNames.Clear();
    }

    private void SelectAllClicked()
    {
        _selectedImageNames.Clear();
        if (_activePanelIndex == 0)
        {
            _selectedImageNames.AddRange(_imageCollectionMeta.UnannotatedFileNames);
        }
        else
        {
            _selectedImageNames.AddRange(_imageCollectionMeta.AnnotatedFileNames);
        }
    }

    private async Task AnnotateClicked()
    {
        if (!_selectedImageNames.Any())
        {
            return;
        }

        await ShowAnnotateAsync(_selectedImageNames[0]);
    }

    private async Task AddToDatasetClicked()
    {
        Console.WriteLine(_selectedImageNames.Count);
        try
        {
            foreach (string imageName in _selectedImageNames)
            {
                await DatasetManagerService.AddImageAsync(new DatasetManagerService.AddImageParameters(ProjectId, string.Empty, imageName));
            }
        }
        catch (RpcException)
        {
            Snackbar.Add("Failed to add images to dataset!", Severity.Warning);
        }
    }

    private async Task ThumbnailAnnotateClicked(string value)
    {
        await ShowAnnotateAsync(value);
    }

    private async ValueTask ShowAnnotateAsync(string selectedImageName)
    {
        await StateService.SetNetStateAsync(StateService.NetState with { Annotation = new Web.Shared.Models.UiNetState.AnnotationState(_selectedImageNames.OrderBy(n => n), 0) });

        NavigationManager.NavigateTo($"net/browse/{ProjectId}/annotate/{selectedImageName}");
    }

    private async Task FetchImageCollectionMeta()
    {
        try
        {
            await UpdateImageCollectionMeta(_selectedBatchName, _selectedGroupName, _selectedTags);

        }
        catch (RpcException)
        {
            Snackbar.Add("Failed to fetch image data!", Severity.Warning);
        }
        finally
        {
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private enum SplitGroup
    {
        All,
        Train,
        Valid,
        Test
    }
}
