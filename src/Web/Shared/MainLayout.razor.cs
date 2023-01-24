using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Utilities;

namespace AyBorg.Web.Shared;

public sealed partial class MainLayout : LayoutComponentBase, IDisposable
{
    [Inject] NavigationManager NavigationManager { get; set; } = null!;
    [Inject] IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] ILocalStorageService LocalStorageService { get; set; } = null!;
    public string RouteName = string.Empty;

    private bool _isDarkMode = true;
    private bool _isDrawerOpen = true;
    private bool _isDisposed = false;

    private readonly MudTheme _theme = new();

    protected override void OnInitialized()
    {
        CreateTheme();
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
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            NavigationManager.LocationChanged -= HandleLocationChanged;
            _isDisposed = true;
        }
    }

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        RouteName = args.Location;
        StateHasChanged();
    }

    private async void OnThemeChanged(bool value)
    {
        _isDarkMode = !_isDarkMode;
        await JsRuntime.InvokeVoidAsync("switchTheme", _isDarkMode);
        await LocalStorageService.SetItemAsync("Theme.IsDarkModeDisabled", !_isDarkMode);
    }

    private void DrawerToggle()
    {
        _isDrawerOpen = !_isDrawerOpen;
    }

    private void CreateTheme()
    {
        // Light Theme
        _theme.Palette.AppbarText = new MudColor("#424242ff");
        _theme.Palette.AppbarBackground = new MudColor("#00000000");
        _theme.Palette.Background = new MudColor("#f9f9f9");
        _theme.Palette.DrawerBackground = new MudColor("#f9f9f9");

        // Dark Theme
        _theme.PaletteDark.TextPrimary = new MudColor("#f0f0f0");
        _theme.PaletteDark.Info = new MudColor("#00BCD4");
        _theme.PaletteDark.InfoDarken = "#00a1b6";
        _theme.PaletteDark.Background = new MudColor("#1a1a27ff");
        _theme.PaletteDark.BackgroundGrey = new MudColor("#252532");
        _theme.PaletteDark.DrawerBackground = new MudColor("#1a1a27ff");
        _theme.PaletteDark.DrawerText = new MudColor("#f0f0f0");
        _theme.PaletteDark.AppbarBackground = new MudColor("#1a1a27cc");
        _theme.PaletteDark.AppbarText = new MudColor("#f0f0f0");
        _theme.PaletteDark.Surface = new MudColor("#1e1e2dff");

        _theme.Shadows.Elevation[1] = "0 2px 4px -1px rgba(6, 24, 44, 0.2)";
        _theme.LayoutProperties.DefaultBorderRadius = "6px";

        _theme.Typography.H6.FontSize = "1.125rem";
        _theme.Typography.H6.FontWeight = 700;
        _theme.Typography.H6.LineHeight = 2d;
    }
}
