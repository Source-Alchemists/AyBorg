using System.Collections.ObjectModel;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class CollectionField : ComponentBase
{
    private List<object> _values = null!;

    [Parameter, EditorRequired] public Port Port { get; set; } = null!;

    [Parameter] public bool Disabled { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        switch (Port.Brand)
        {
            case PortBrand.StringCollection:
                _values = new List<object>((ReadOnlyCollection<string>)Port.Value!).Cast<object>().ToList();
                break;
            default:
                throw new InvalidOperationException($"Port {Port.Name} is not a collection.");

        }
    }
}
