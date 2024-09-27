using MudBlazor;
using MudBlazor.Utilities;

namespace AyBorg.Web.Components.Layout;

public sealed class ThemeFactory
{
    public static MudTheme Create()
    {
        var theme = new MudTheme();
        // Light Theme
        theme.Palette.Secondary = new MudColor("#af4ae2");
        theme.Palette.Info = new MudColor("#00BCD4");
        theme.Palette.InfoDarken = "#00a1b6";
        theme.Palette.AppbarText = new MudColor("#424242ff");
        theme.Palette.AppbarBackground = new MudColor("#00000000");
        theme.Palette.Background = new MudColor("#f9f9f9");
        theme.Palette.DrawerBackground = new MudColor("#f9f9f9");

        // Dark Theme
        theme.PaletteDark.TextPrimary = new MudColor("#f0f0f0");
        theme.PaletteDark.Secondary = new MudColor("#af4ae2");
        theme.PaletteDark.Info = new MudColor("#00BCD4");
        theme.PaletteDark.InfoDarken = "#00a1b6";
        theme.PaletteDark.Background = new MudColor("#1a1a27ff");
        theme.PaletteDark.BackgroundGrey = new MudColor("#252532");
        theme.PaletteDark.DrawerBackground = new MudColor("#1a1a27ff");
        theme.PaletteDark.DrawerText = new MudColor("#f0f0f0");
        theme.PaletteDark.AppbarBackground = new MudColor("#1a1a27cc");
        theme.PaletteDark.AppbarText = new MudColor("#f0f0f0");
        theme.PaletteDark.Surface = new MudColor("#1e1e2dff");

        theme.Shadows.Elevation[1] = "0 2px 4px -1px rgba(6, 24, 44, 0.2)";
        theme.LayoutProperties.DefaultBorderRadius = "6px";

        theme.Typography.H6.FontSize = "1.125rem";
        theme.Typography.H6.FontWeight = 700;
        theme.Typography.H6.LineHeight = 2d;

        return theme;
    }
}
