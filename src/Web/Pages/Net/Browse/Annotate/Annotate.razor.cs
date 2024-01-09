using AyBorg.Web.Services;
using AyBorg.Web.Services.Net;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Net.Browse.Annotate;

public partial class Annotate : ComponentBase
{
    [Parameter] public string ProjectId { get; init; } = string.Empty;
    [Parameter] public string ImageName { get; init; } = string.Empty;

    [Inject] IProjectManagerService ProjectManagerService { get; init; } = null!;
    [Inject] IFileManagerService FileManagerService { get; init; } = null!;
    [Inject] IStateService StateService { get; init; } = null!;
    [Inject] NavigationManager NavigationManager { get; init; } = null!;

    private string _projectName = string.Empty;
    private bool _isLoading = true;
    private IEnumerable<string> _selectedImageNames = Array.Empty<string>();
    private int _selectedImageNumber = 1;
    private string _lastImageName = string.Empty;
    private FileManagerService.ImageContainer _imageContainer = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            if(StateService.NetState != null)
            {
                _projectName = StateService.NetState.ProjectName;
                _selectedImageNames = StateService.NetState.Annotation.SelectedImageNames;
                _selectedImageNumber = StateService.NetState.Annotation.SelectedImageIndex + 1;
            }

            // Fallback
            if (string.IsNullOrEmpty(_projectName))
            {
                IEnumerable<Shared.Models.Net.ProjectMeta> projectMetas = await ProjectManagerService.GetMetasAsync();
                Shared.Models.Net.ProjectMeta? targetProjectMeta = projectMetas.FirstOrDefault(p => p.Id.Equals(ProjectId, StringComparison.InvariantCultureIgnoreCase));
                _projectName = targetProjectMeta != null ? targetProjectMeta.Name : "Unknown";
            }

            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        
        if(_lastImageName.Equals(ImageName, StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        _imageContainer = await FileManagerService.DownloadImageAsync(new FileManagerService.DownloadImageParameters(ProjectId, ImageName, false));

        await InvokeAsync(StateHasChanged);
    }

    private async Task SelectedImageNumberChanged(int value)
    {
        int index = value - 1;
        _selectedImageNumber = value;
        await StateService.SetNetStateAsync(StateService.NetState with { Annotation = StateService.NetState.Annotation with { SelectedImageIndex = index } });
        NavigationManager.NavigateTo($"net/browse/{ProjectId}/annotate/{_selectedImageNames.ElementAt(index)}");
    }
}