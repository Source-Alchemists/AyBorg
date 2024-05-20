using AyBorg.Web.Services;
using AyBorg.Web.Services.Cognitive;
using AyBorg.Web.Shared.Models;
using AyBorg.Web.Shared.Models.Cognitive;
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Cognitive.Models;

public partial class ModelInspect : ComponentBase
{
    [Parameter] public string ProjectId { get; init; } = string.Empty;
    [Parameter] public string ModelId { get; init; } = string.Empty;
    [Inject] IStateService StateService { get; init; } = null!;
    [Inject] IProjectManagerService ProjectManagerService { get; init; } = null!;
    [Inject] IFileManagerService FileManagerService { get; init; } = null!;
    [Inject] ISnackbar Snackbar { get; init; } = null!;
    private bool _isLoading = true;
    private string _projectName = string.Empty;
    private string _modelName = string.Empty;
    private HashSet<ImageSource> _imageSources = new();
    private ImageSource _selectedImageSource = null!;
    private bool _hasUserInteraction = false;
    private FileManagerService.ModelMeta _modelMeta = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await StateService.UpdateStateFromSessionStorageAsync();

            if (StateService.CognitiveState != null)
            {
                _projectName = StateService.CognitiveState.ProjectName;
            }
            else
            {
                try
                {
                    IEnumerable<ProjectMeta> projectMetas = await ProjectManagerService.GetMetasAsync();
                    ProjectMeta? targetMeta = projectMetas.FirstOrDefault(m => m.Id.Equals(ProjectId, StringComparison.InvariantCultureIgnoreCase));
                    _projectName = targetMeta != null ? targetMeta.Name : string.Empty;
                }
                catch (RpcException)
                {
                    Snackbar.Add("Failed to get project informations!", Severity.Warning);
                }
            }

            try
            {
                IEnumerable<FileManagerService.ModelMeta> modelMetas = await FileManagerService.GetModelMetasAsync(new FileManagerService.GetModelMetasParameters(
                    ProjectId: ProjectId
                ));
                _modelMeta = modelMetas.First(m => m.Id.Equals(ModelId));
                _modelName = _modelMeta.Name;
            }
            catch (RpcException)
            {
                Snackbar.Add("Failed to get model informations!", Severity.Warning);
            }

            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ImageAdded(ImageSource image)
    {
        if (_imageSources.Any(i => i.Hash.Equals(image.Hash)))
        {
            return;
        }

        _imageSources.Add(image);
        if (!_hasUserInteraction)
        {
            _selectedImageSource = image;
        }

        await InvokeAsync(StateHasChanged);
    }

    private void ImageSelectionChanged(ImageSource image)
    {
        _selectedImageSource = image;
        _hasUserInteraction = true;
    }
}
