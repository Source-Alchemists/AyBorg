using AyBorg.Web.Services;
using AyBorg.Web.Services.Net;
using AyBorg.Web.Shared.Models.Net;
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Net.Datasets;

public partial class Datasets : ComponentBase
{
    [Parameter] public string ProjectId { get; init; } = string.Empty;
    [Inject] IStateService StateService { get; init; } = null!;
    [Inject] IProjectManagerService ProjectManagerService { get; init; } = null!;
    [Inject] IDatasetManagerService DatasetManagerService { get; init; } = null!;
    [Inject] ISnackbar Snackbar { get; init; } = null!;
    [Inject] IDialogService DialogService { get; init; } = null!;
    private string _projectName = string.Empty;
    private bool _isLoading = true;
    private DatasetMeta _tempDataset = new();
    private DatasetMeta _activeDataset = new();
    private IEnumerable<DatasetMeta> _completedDatasets = Array.Empty<DatasetMeta>();
    private float _trainDistribution = float.NaN;
    private float _valDistribution = float.NaN;
    private float _testDistribution = float.NaN;
    private bool _isSaveDisabled = true;
    private bool _isGenerateDisabled = true;

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
                    IEnumerable<ProjectMeta> metas = await ProjectManagerService.GetMetasAsync();
                    ProjectMeta? targetMeta = metas.FirstOrDefault(m => m.Id.Equals(ProjectId, StringComparison.InvariantCultureIgnoreCase));
                    _projectName = targetMeta != null ? targetMeta.Name : string.Empty;
                }
                catch (RpcException)
                {
                    Snackbar.Add("Failed to get project informations!", Severity.Warning);
                }
            }

            await UpdateMetaCollectionAsync();

            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task UpdateMetaCollectionAsync()
    {
        try
        {
            IEnumerable<DatasetMeta> datasetMetas = await DatasetManagerService.GetMetasAsync(new DatasetManagerService.GetMetasParameters(ProjectId));
            _activeDataset = datasetMetas.First(d => d.IsActive);
            _completedDatasets = datasetMetas.Where(d => !d.IsActive);
            _tempDataset = _activeDataset with { };
            CalcDistribution();
            UpdateGenerateButton();
        }
        catch (RpcException)
        {
            Snackbar.Add("Failed to get dataset informantions!", Severity.Warning);
        }
    }

    private void CalcDistribution()
    {
        int train = _activeDataset.Distribution.ElementAt(0);
        int val = _activeDataset.Distribution.ElementAt(1);
        int test = _activeDataset.Distribution.ElementAt(2);
        int sum = train + val + test;

        float f = 100f / sum;
        _trainDistribution = f * train;
        _valDistribution = f * val;
        _testDistribution = f * test;
    }

    private void UpdateSaveButton()
    {
        _isSaveDisabled = string.IsNullOrEmpty(_tempDataset.Name)
                            || string.IsNullOrWhiteSpace(_tempDataset.Name)
                            || _tempDataset.Equals(_activeDataset);
    }

    private void UpdateGenerateButton()
    {
        int train = _activeDataset.Distribution.ElementAt(0);
        int val = _activeDataset.Distribution.ElementAt(1);
        int test = _activeDataset.Distribution.ElementAt(2);
        int sum = train + val + test;
        _isGenerateDisabled = sum <= 0;
    }

    private void NameTextChanged(string value)
    {
        _tempDataset = _tempDataset with { Name = value };
        UpdateSaveButton();
    }

    private void CommentTextChanged(string value)
    {
        _tempDataset = _tempDataset with { Comment = value };
        UpdateSaveButton();
    }

    private async Task SaveChangesClicked()
    {
        try
        {
            await DatasetManagerService.EditAsync(new DatasetManagerService.EditParameters(
                ProjectId,
                _tempDataset
            ));

            _activeDataset = _tempDataset with { };
        }
        catch (RpcException)
        {
            Snackbar.Add("Failed to edit dataset!", Severity.Warning);
        }
    }

    private async Task NewDraftClicked()
    {
        IDialogReference dialogReference = DialogService.Show<NewDatasetDialog>("New Dataset Draft", new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseButton = true
        });

        DialogResult result = await dialogReference.Result;
        if (!result.Canceled)
        {
            var res = (NewDatasetDialog.Result)result.Data;
            DatasetMeta newDatasetMeta = await DatasetManagerService.CreateAsync(new DatasetManagerService.CreateParameters(ProjectId, res.Withdraw));
            _activeDataset = newDatasetMeta;
            _tempDataset = _activeDataset with { };
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task GenerateClicked()
    {
        IDialogReference dialogReference = DialogService.Show<GenerateDatasetDialog>("Generate Dataset", new DialogParameters {
            { "ProjectId", ProjectId },
            { "DatasetId", _tempDataset.Id }
        }, new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseButton = false
        });

        DialogResult result = await dialogReference.Result;
        if (!result.Canceled)
        {
            _isLoading = true;
            await UpdateMetaCollectionAsync();
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}
