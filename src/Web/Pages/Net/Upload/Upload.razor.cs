using AyBorg.Web.Services;
using AyBorg.Web.Services.Net;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Net.Upload;

public partial class Upload : ComponentBase
{
    [Parameter] public string ProjectId { get; init; } = string.Empty;
    [Inject] IStateService StateService { get; init; } = null!;
    [Inject] IProjectManagerService ProjectManagerService { get; init; } = null!;

    private string _projectName = string.Empty;

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
                IEnumerable<Shared.Models.Net.ProjectMeta> metas = await ProjectManagerService.GetMetasAsync();
                Shared.Models.Net.ProjectMeta? targetMeta = metas.FirstOrDefault(m => m.Id.Equals(ProjectId, StringComparison.InvariantCultureIgnoreCase));
                if(targetMeta != null)
                {
                    _projectName = targetMeta.Name;
                    await StateService.SetNetStateAsync(new Shared.Models.UiNetState(targetMeta));
                }
            }

            await InvokeAsync(StateHasChanged);
        }
    }
}