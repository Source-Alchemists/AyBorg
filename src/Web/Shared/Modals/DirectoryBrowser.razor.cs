using AyBorg.Web.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Shared.Modals;

public partial class DirectoryBrowser : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    [Inject] protected IStorageService StorageService { get; set; } = null!;
    [Parameter] public string? RootPath { get; set; }

    private readonly HashSet<DirectoryItem> _items = new();
    private DirectoryItem? _selectedItem;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        _items.Clear();
        _selectedItem = null;
        foreach (string dir in await StorageService!.GetDirectoriesAsync("/"))
        {
            DirectoryItem item = await CreateDirectoryItemAsync(dir);
            _items.Add(item);
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task<DirectoryItem> CreateDirectoryItemAsync(string dir)
    {
        string name = Path.GetFileName(dir);
        var childs = new List<DirectoryItem>();
        if (dir != "/")
        {
            IEnumerable<string> subDirs = await StorageService!.GetDirectoriesAsync(dir);
            foreach (string subDir in subDirs)
            {
                DirectoryItem subItem = await CreateDirectoryItemAsync(subDir);
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
