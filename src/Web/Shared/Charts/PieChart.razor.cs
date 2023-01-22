using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Shared.Charts;

public partial class PieChart : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public string[] Labels { get; set; } = Array.Empty<string>();

    [Parameter]
    [EditorRequired]
    public double[] Data { get; set; } = Array.Empty<double>();

    [Parameter]
    public string Width { get; set; } = "200px";

    [Parameter]
    public string Height { get; set; } = "200px";

    protected override void OnParametersSet() {
         base.OnParametersSet();
         StateHasChanged();
    }
}
