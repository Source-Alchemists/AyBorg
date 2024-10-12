using AyBorg.Web.Services;
using AyBorg.Web.Services.Cognitive;
using AyBorg.Web.Shared.Models.Cognitive;
using AyBorg.Web.Pages.Cognitive.Shared;
using AyBorg.Web.Shared.Modals;
using AyBorg.Types;

using Grpc.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Collections.Immutable;

namespace AyBorg.Web.Pages.Cognitive.Models;

public partial class Models : ComponentBase, IAsyncDisposable
{
    [Parameter] public string ProjectId { get; init; } = string.Empty;
    [Inject] ILogger<Models> Logger { get; init; } = null!;
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
                catch (RpcException ex)
                {
                    Logger.LogWarning((int)EventLogType.UserInteraction, ex, "Failed to get project informations!");
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
        catch (RpcException ex)
        {
            Logger.LogWarning((int)EventLogType.UserInteraction, ex, "Failed to get database informations!");
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
        IDialogReference dialog = await DialogService.ShowAsync<StartModelTrainingDialog>("Model Training", new DialogParameters {
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
                NavigationManager.NavigateTo("cognitive/jobs");
            }
            catch (RpcException ex)
            {
                Logger.LogWarning((int)EventLogType.UserInteraction, ex, "Failed to start model training!");
                Snackbar.Add("Failed to start model training!", Severity.Warning);
            }
        }
    }

    private async Task EditModelClicked(FileManagerService.ModelMeta modelMeta)
    {
        IDialogReference dialog = await DialogService.ShowAsync<EditModelDialog>("Edit Model", new DialogParameters {
            { "Name", modelMeta.Name }
        }, new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        });

        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            string newName = (string)result.Data;
            if (newName.Equals(modelMeta.Name))
            {
                return;
            }

            try
            {
                await FileManagerService.EditModelAsync(new FileManagerService.EditModelParameters(
                    ProjectId: ProjectId,
                    ModelId: modelMeta.Id,
                    OldName: modelMeta.Name,
                    NewName: newName
                ));
                _isLoading = true;
                await UpdateMetasAsync();
                _isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
            catch (RpcException ex)
            {
                Logger.LogWarning((int)EventLogType.UserInteraction, ex, "Failed to edit model!");
                Snackbar.Add("Failed to edit model!");
            }
        }
    }

    private async Task DeleteModelClicked(FileManagerService.ModelMeta modelMeta)
    {
        IDialogReference dialog = await DialogService.ShowAsync<ConfirmDialog>("Delete Model", new DialogParameters {
            { "NeedPassword", true },
            { "ContentText", $"Are you sure you want to delete model '{modelMeta.Name}'? This action cannot be undone."}
        });

        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            try
            {
                await FileManagerService.DeleteModelAsync(new Services.Cognitive.FileManagerService.DeleteModelParameters(
                    ProjectId: ProjectId,
                    ModelId: modelMeta.Id,
                    ModelName: modelMeta.Name
                ));
                _isLoading = true;
                await UpdateMetasAsync();
                _isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
            catch (RpcException ex)
            {
                Logger.LogWarning((int)EventLogType.UserInteraction, ex, "Failed to delete model [{ModelName}]", modelMeta.Name);
                Snackbar.Add($"Failed to delete model [{modelMeta.Name}]", Severity.Warning);
            }
        }
    }

    private async Task CreateReviewClicked(FileManagerService.ModelMeta modelMeta)
    {
        IDialogReference dialog = await DialogService.ShowAsync<ConfirmDialog>($"Create Review for Model {modelMeta.Name}",
        new DialogParameters {
            { "ShowComment", true },
            { "Comment", modelMeta.Comment },
            { "NeedPassword", true }
        },
        new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        });

        await ChangeModelState(modelMeta, dialog, Services.Cognitive.FileManagerService.ModelState.Review);
    }

    private async Task ApproveClicked(FileManagerService.ModelMeta modelMeta)
    {
        IDialogReference dialog = await DialogService.ShowAsync<ConfirmDialog>($"Approve Model {modelMeta.Name}",
        new DialogParameters {
            { "ShowComment", true },
            { "Comment", modelMeta.Comment },
            { "NeedPassword", true }
        },
        new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        });
        await ChangeModelState(modelMeta, dialog, Services.Cognitive.FileManagerService.ModelState.Release);
    }

    private async Task AbandonClicked(FileManagerService.ModelMeta modelMeta)
    {
        IDialogReference dialog = await DialogService.ShowAsync<ConfirmDialog>($"Abandon Review for Model {modelMeta.Name}",
        new DialogParameters {
            { "ContentText", $"Are you sure you want to abandon review for model '{modelMeta.Name}'?" },
            { "Comment", "Abandoned Review" },
            { "NeedPassword", true }
        },
        new DialogOptions {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        });

        await ChangeModelState(modelMeta, dialog, Services.Cognitive.FileManagerService.ModelState.Draft);
    }

    private void InspectClicked(FileManagerService.ModelMeta modelMeta)
    {
        NavigationManager.NavigateTo($"cognitive/models/{ProjectId}/inspect/{modelMeta.Id}");
    }

    private async Task ChangeModelState(FileManagerService.ModelMeta modelMeta, IDialogReference dialog, FileManagerService.ModelState NewModelSate)
    {
        DialogResult dialogResult = await dialog.Result;
        if (!dialogResult.Canceled)
        {
            var result = (ConfirmDialog.ConfirmResult)dialogResult.Data;
            try
            {
                await FileManagerService.ChangeModelStateAsync(new Services.Cognitive.FileManagerService.ChangeModelStateParameters(
                    ProjectId: ProjectId,
                    ModelId: modelMeta.Id,
                    ModelName: modelMeta.Name,
                    OldState: modelMeta.State,
                    NewState: NewModelSate,
                    Comment: result.Comment
                ));
                _isLoading = true;
                await UpdateMetasAsync();
                _isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
            catch (RpcException ex)
            {
                Logger.LogWarning((int)EventLogType.UserInteraction, ex, "Failed to change model state!");
                Snackbar.Add("Failed to change model state!", Severity.Warning);
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        _updateTimer?.Dispose();
        return ValueTask.CompletedTask;
    }
}
