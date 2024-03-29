using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Shared.Charts;

public partial class LineChart : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public Dictionary<object, List<double>> SeriesData { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public List<string> SeriesLabels { get; set; } = null!;

    [Parameter]
    public string Width { get; set; } = "100%";

    [Parameter]
    public string Height { get; set; } = "100%";

    private bool _isLoading = true;
    private readonly List<ChartSeries> _series = new();
    private readonly ChartOptions _options = new();
    private string[] _labels = Array.Empty<string>();

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _options.YAxisTicks = 1;
        _options.XAxisLines = false;
        _series.Clear();
        _labels = SeriesLabels.ToArray();
        if (_labels != Array.Empty<string>())
        {
            _isLoading = false;
        }
        foreach (object key in SeriesData.Keys)
        {
            double[] data = SeriesData[key].ToArray();
            _series.Add(new ChartSeries
            {
                Name = key.ToString(),
                Data = data
            });
        }
    }
}
