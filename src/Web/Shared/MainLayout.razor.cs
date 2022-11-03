using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;

namespace Atomy.Web.Shared;

public partial class MainLayout : LayoutComponentBase
{
    [Inject]
    protected NavigationManager _navigationManager { get; set; } = null!;
    [Inject]
    protected IJSRuntime _jsRuntime { get; set; } = null!;
    [Inject]
    protected ILocalStorageService _localStorageService { get; set; } = null!;
    public string RouteName = string.Empty;

    private bool _isDarkMode = true;
    private bool _isDrawerOpen = true;
    private MudSwitch<bool>? _themeSwitchRef;
    private MudTheme _theme = new MudTheme() { 
        Palette = new Palette() { 
            Info = "#00BCD4",
        }
    };

    protected override void OnInitialized()
    {
        RouteName = _navigationManager.Uri;
        _navigationManager.LocationChanged += HandleLocationChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isDarkMode = !(await _localStorageService.GetItemAsync<bool>("Theme.IsDarkModeDisabled"));
            await _jsRuntime.InvokeVoidAsync("switchTheme", _isDarkMode);
            await InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        _navigationManager.LocationChanged -= HandleLocationChanged;
    }

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        RouteName = args.Location;
        StateHasChanged();
    }

    private async void OnThemeSwitchChanged(bool value)
    {
        _isDarkMode = !_isDarkMode;
        await _jsRuntime.InvokeVoidAsync("switchTheme", _isDarkMode);
        await _localStorageService.SetItemAsync("Theme.IsDarkModeDisabled", !_isDarkMode);
    }

    private void DrawerToggle()
    {
        _isDrawerOpen = !_isDrawerOpen;
    }
}