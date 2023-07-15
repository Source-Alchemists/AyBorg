using AyBorg.Web.Shared.Models.Agent;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Agent.Devices;

public partial class DeviceSettingsDialog : ComponentBase
{
    private bool _isLoading = true;
    [Parameter] public DeviceMeta Device { get; init; } = null!;
}
