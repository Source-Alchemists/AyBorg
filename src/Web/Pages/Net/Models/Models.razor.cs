using AyBorg.Web.Services;
using AyBorg.Web.Services.Net;
using AyBorg.Web.Shared.Models.Net;
using AyBorg.Web.Pages.Net.Shared;
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Collections.Immutable;
using AyBorg.Web.Shared.Modals;

namespace AyBorg.Web.Pages.Net.Models;

public partial class Models : ComponentBase, IAsyncDisposable
{
    [Parameter] public string ProjectId { get; init; } = string.Empty;
    [Inject] IStateService StateService { get; init; } = null!;
    [Inject] IProjectManagerService ProjectManagerService { get; init; } = null!;
    [Inject] IJobManagerService JobManagerService { get; init; } = null!;
    [Inject] IFileManagerService FileManagerService { get; init; } = null!;
    [Inject] IDatasetManagerService DatasetManagerService { get; init; } = null!;
    [Inject] ISnackbar Snackbar { get; init; } = null!;
    [Inject] IDialogService DialogService { get; init; } = null!;
    [Inject] NavigationManager NavigationManager { get; init; } = null!;

    private bool _isLoading = true;
    private Timer _updateTimer = null!;
    private string _projectName = string.Empty;
    private ImmutableList<FileManagerService.ModelMeta> _modelMetas = ImmutableList<FileManagerService.ModelMeta>.Empty;
    private IEnumerable<DatasetMeta> _datasetMetas = Array.Empty<DatasetMeta>();
    private bool _isTrainingDisabled => !_datasetMetas.Any(d => !d.IsActive);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await StateService.UpdateStateFromSessionStorageAsync();

            if (StateService.NetState != null)
            {
                _projectName = StateService.NetState.ProjectName;
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

            _updateTimer = new Timer(async e =>
            {
                if (_isLoading) return;
                await UpdateMetasAsync();
                await InvokeAsync(StateHasChanged);
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            await UpdateMetasAsync();
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async ValueTask UpdateMetasAsync()
    {
        try
        {
            _datasetMetas = await DatasetManagerService.GetMetasAsync(new DatasetManagerService.GetMetasParameters(ProjectId));
        }
        catch (RpcException)
        {
            Snackbar.Add("Failed to get database informations!", Severity.Warning);
            return;
        }

        try
        {
            IEnumerable<FileManagerService.ModelMeta> modelMetas = await FileManagerService.GetModelMetasAsync(new FileManagerService.GetModelMetasParameters(
                ProjectId: ProjectId
            ));
            _modelMetas = _modelMetas.Clear();
            _modelMetas = _modelMetas.AddRange(modelMetas.OrderByDescending(m => m.CreationDate));
        }
        catch (RpcException)
        {
            Snackbar.Add("Failed to get model informations!", Severity.Warning);
        }
    }

    private async Task TrainClicked()
    {
        IDialogReference dialog = DialogService.Show<StartModelTrainingDialog>("Model Training", new DialogParameters {
            { "ShowDatasetSelection", true },
            { "Name", "AyBorg Object Detection" },
            { "ProjectId", ProjectId }
        }, new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        });

        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            var trainingParameters = (StartModelTrainingDialog.TrainParameters)result.Data;
            try
            {
                await JobManagerService.CreateAsync(new JobManagerService.CreateJobParameters(
                    ProjectId: ProjectId,
                    DatasetId: trainingParameters.DatasetId,
                    ModelName: trainingParameters.ModelName,
                    Iterations: trainingParameters.Iterations
                ));
                Snackbar.Add("Model Training started", Severity.Info);
                NavigationManager.NavigateTo("net/jobs");
            }
            catch (RpcException)
            {
                Snackbar.Add("Failed to start Model Training!", Severity.Warning);
            }
        }
    }

    private async Task EditModel(string modelId, string modelName)
    {
        IDialogReference dialog = DialogService.Show<EditModelDialog>("Edit Model", new DialogParameters {
            { "Name", modelName }
        }, new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        });

        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            var newName = (string)result.Data;
            if (newName.Equals(modelName))
            {
                return;
            }

            try
            {
                await FileManagerService.EditModelAsync(new Services.Net.FileManagerService.EditModelParameters(
                    ProjectId: ProjectId,
                    ModelId: modelId,
                    OldName: modelName,
                    NewName: newName
                ));
                _isLoading = true;
                await UpdateMetasAsync();
                _isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
            catch (RpcException)
            {
                Snackbar.Add("Failed to edit model!");
            }
        }
    }

    private async Task DeleteModel(string modelId, string modelName)
    {
        IDialogReference dialog = DialogService.Show<ConfirmDialog>("Delete Model", new DialogParameters {
            { "NeedPassword", true },
            { "ContentText", $"Are you sure you want to delete model '{modelName}'? This action cannot be undone."}
        });

        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            try
            {
                await FileManagerService.DeleteModelAsync(new Services.Net.FileManagerService.DeleteModelParameters(
                    ProjectId: ProjectId,
                    ModelId: modelId,
                    ModelName: modelName
                ));
                _isLoading = true;
                await UpdateMetasAsync();
                _isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
            catch (RpcException)
            {
                Snackbar.Add($"Failed to delete model [{modelName}]", Severity.Warning);
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        _updateTimer?.Dispose();
        return ValueTask.CompletedTask;
    }
}
