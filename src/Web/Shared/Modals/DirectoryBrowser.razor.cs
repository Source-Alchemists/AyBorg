using Autodroid.Web.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Autodroid.Web.Shared.Modals;

public partial class DirectoryBrowser : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    [Inject] protected IStorageService StorageService { get; set; } = null!;
    [Inject] protected IStateService StateService { get; set; } = null!;
    [Parameter] public string? RootPath { get; set; }
    
    private readonly HashSet<DirectoryItem> _items = new();
    private DirectoryItem? _selectedItem;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        _items.Clear();
        _selectedItem = null;
        foreach (var dir in await StorageService!.GetDirectoriesAsync(StateService.AgentState.BaseUrl, "/"))
        {
            var item = await CreateDirectoryItemAsync(dir);
            _items.Add(item);
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task<DirectoryItem> CreateDirectoryItemAsync(string dir)
    {
        var name = Path.GetFileName(dir);
        var childs = new List<DirectoryItem>();
        if (dir != "/")
        {
            var subDirs = await StorageService!.GetDirectoriesAsync(StateService.AgentState.BaseUrl, dir);
            foreach (var subDir in subDirs)
            {
                var subItem = await CreateDirectoryItemAsync(subDir);
                childs.Add(subItem);
            }
        }

        return new DirectoryItem { Name = name, Value = dir, Children = new HashSet<DirectoryItem>(childs) };
    }

    private void OnChooseClicked()
    {
        MudDialog.Close(DialogResult.Ok(_selectedItem?.Value));
    }

    private void OnCancelClicked()
    {
        MudDialog.Cancel();
    }

    public class DirectoryItem
    {
        public string Name { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public HashSet<DirectoryItem> Children { get; init; } = new HashSet<DirectoryItem>();
    }
}