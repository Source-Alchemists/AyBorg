using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;

namespace Autodroid.Web.Shared;

public partial class MainLayout : LayoutComponentBase
{
    [Inject]
    protected NavigationManager NavigationManager { get; set; } = null!;
    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = null!;
    [Inject]
    protected ILocalStorageService LocalStorageService { get; set; } = null!;
    public string RouteName = string.Empty;

    private bool _isDarkMode = true;
    private bool _isDrawerOpen = true;
    private readonly MudTheme _theme = new() { 
        Palette = new Palette() { 
            Info = "#00BCD4",
        }
    };

    protected override void OnInitialized()
    {
        RouteName = NavigationManager.Uri;
        NavigationManager.LocationChanged += HandleLocationChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isDarkMode = !(await LocalStorageService.GetItemAsync<bool>("Theme.IsDarkModeDisabled"));
            await JsRuntime.InvokeVoidAsync("switchTheme", _isDarkMode);
            await InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= HandleLocationChanged;
    }

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        RouteName = args.Location;
        StateHasChanged();
    }

    private async void OnThemeSwitchChanged(bool value)
    {
        _isDarkMode = !_isDarkMode;
        await JsRuntime.InvokeVoidAsync("switchTheme", _isDarkMode);
        await LocalStorageService.SetItemAsync("Theme.IsDarkModeDisabled", !_isDarkMode);
    }

    private void DrawerToggle()
    {
        _isDrawerOpen = !_isDrawerOpen;
    }
}