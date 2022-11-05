
using Atomy.Web.Shared.Modals;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Atomy.Web.Pages.Agent.Shared.Fields;

public partial class DirectoryInputField : BaseInputField
{
    private string _value = string.Empty;

    [Inject] IDialogService DialogService { get; set; } = null!;

    protected override Task OnInitializedAsync()
    {
        _value = Convert.ToString(Port.Value)!;
        return base.OnInitializedAsync();
    }

    protected override async void OnEditFieldClicked()
    {
        var options = new DialogOptions()
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };
        var parameters = new DialogParameters
        {
            { "RootPath", _value }
        };
        var dialog = DialogService.Show<DirectoryBrowser>("Directory browser", parameters, options);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            _value = result.Data.ToString()!;
            Port.Value = _value;
            await NotifyValueChangedAsync(_value);
        }
    }
}