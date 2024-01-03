using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Net.Upload;

public partial class ImagesDistributionDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    private IEnumerable<DistributionMethod> _selectedDistributionMethod = new List<DistributionMethod> { DistributionMethod.TrainValidTest };
    private bool _isRange = true;
    private int _trainSliderValue = 70;
    private int _validSliderValue = 90;
    private int _trainFactor => _trainSliderValue;
    private int _validFactor => _selectedDistributionMethod.First().Equals(DistributionMethod.TrainValidTest) ? _validSliderValue - _trainSliderValue : 100 - _trainSliderValue;
    private int _testFactor => 100 - (_trainFactor + _validFactor);

    private void DistributionMethodChanged(DistributionMethod value)
    {
        _selectedDistributionMethod = new List<DistributionMethod> { value };

        if (value == DistributionMethod.TrainValid)
        {
            _isRange = false;
        }
        else
        {
            _isRange = true;
        }
    }

    private void OnContinueClicked()
    {
        MudDialog.Close(DialogResult.Ok(new Result(_trainFactor, _validFactor, _testFactor)));
    }

    private void OnCancelClicked()
    {
        MudDialog.Cancel();
    }

    private enum DistributionMethod
    {
        TrainValidTest = 1,
        TrainValid = 2
    }

    public sealed record Result(int TrainFactor, int ValidFactor, int TestFactor);
}