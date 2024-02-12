using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Net.Shared;

public partial class StartModelTrainingDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; init; } = null!;
    [Parameter] public bool ShowDatasetSelection { get; init; } = false;

    private int _iterations = 10;

    private void CancelClicked()
    {
        MudDialog.Cancel();
    }

    private void StartClicked()
    {
        MudDialog.Close(DialogResult.Ok(new TrainParameters(
            Iterations: _iterations,
            DatasetId: string.Empty
        )));
    }

    public sealed record TrainParameters(int Iterations, string DatasetId);
}
