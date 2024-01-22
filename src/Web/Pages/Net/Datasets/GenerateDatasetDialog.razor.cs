using AyBorg.Web.Services.Net;
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Net.Datasets;

public partial class GenerateDatasetDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; init; } = null!;
    [Inject] IDatasetManagerService DatasetManagerService { get; init; } = null!;
    [Inject] ISnackbar Snackbar { get; init; } = null!;
    [Parameter] public string ProjectId { get; init; } = string.Empty;
    [Parameter] public string DatasetId { get; init; } = string.Empty;

    private void CancelClicked()
    {
        MudDialog.Cancel();
    }

    private async Task GenerateClicked()
    {
        try
        {
            await DatasetManagerService.GenerateAsync(new DatasetManagerService.GenerateParameters(ProjectId, DatasetId));
        }
        catch (RpcException)
        {
            Snackbar.Add("Failed to generate dataset!", Severity.Warning);
        }
    }
}
