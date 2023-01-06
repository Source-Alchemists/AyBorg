using AyBorg.SDK.Common.Models;
using AyBorg.Web.Services.Agent;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AyBorg.Web.Pages.Agent.Editor;


public partial class StepsSelection : ComponentBase
{
    private IEnumerable<Step> _availableSteps = new List<Step>();

    [Parameter] public string ServiceUniqueName { get; set; } = string.Empty;
    [Inject] PluginsService PluginsService { get; set; } = null!;

    protected override async Task OnParametersSetAsync()
    {
        _availableSteps = await PluginsService.ReceiveStepsAsync(ServiceUniqueName);
        await base.OnParametersSetAsync();
    }

    private static async Task OnDragStart(DragEventArgs _, Step step)
    {
        DragDropStateHandler.DraggedStep = step;
        await Task.CompletedTask;
    }
}
