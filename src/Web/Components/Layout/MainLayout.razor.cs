using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;

namespace AyBorg.Web.Components.Layout;

public sealed partial class MainLayout : LayoutComponentBase, IDisposable
{
    [Inject] NavigationManager NavigationManager { get; set; } = null!;
    [Inject] IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] ILocalStorageService LocalStorageService { get; set; } = null!;
    public string RouteName = string.Empty;

    private bool _isDarkMode = true;
    private bool _isDrawerVisible = false;
    private bool _isDisposed = false;

    private readonly MudTheme _theme = ThemeFactory.Create();

    protected override void OnInitialized()
    {
        UpdateDrawerVisibility();
        RouteName = NavigationManager.Uri;
        NavigationManager.LocationChanged += HandleLocationChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isDarkMode = !await LocalStorageService.GetItemAsync<bool>("Theme.IsDarkModeDisabled");
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
        UpdateDrawerVisibility();
        StateHasChanged();
    }

    private void UpdateDrawerVisibility()
    {
        if (NavigationManager.BaseUri.Equals(NavigationManager.Uri) || NavigationManager.Uri.Contains("/tutorials/"))
        {
            _isDrawerVisible = false;
        }
        else
        {
            _isDrawerVisible = true;
        }
    }

    private async void OnThemeChanged(bool value)
    {
        _isDarkMode = !_isDarkMode;
        await JsRuntime.InvokeVoidAsync("switchTheme", _isDarkMode);
        await LocalStorageService.SetItemAsync("Theme.IsDarkModeDisabled", !_isDarkMode);
    }
}
