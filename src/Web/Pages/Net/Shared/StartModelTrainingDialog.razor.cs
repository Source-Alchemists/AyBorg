using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Net.Shared;

public partial class StartModelTrainingDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; init; } = null!;
    [Parameter] public string Name { get; set; } = string.Empty;
    [Parameter] public bool ShowDatasetSelection { get; init; } = false;
    private bool _isStartDisabled => string.IsNullOrEmpty(Name) || string.IsNullOrWhiteSpace(Name);

    private int _iterations = 10;

    private void CancelClicked()
    {
        MudDialog.Cancel();
    }

    private void StartClicked()
    {
        MudDialog.Close(DialogResult.Ok(new TrainParameters(
            ModelName: Name,
            Iterations: _iterations,
            DatasetId: string.Empty
        )));
    }

    public sealed record TrainParameters(string ModelName, int Iterations, string DatasetId);
}
