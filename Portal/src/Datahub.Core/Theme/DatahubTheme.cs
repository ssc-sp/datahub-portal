using MudBlazor;

namespace Datahub.Core.Theme;

public abstract class DatahubTheme
{
    public static readonly MudTheme DEFAULT_THEME = new()
    {
        Typography =
        {
            Default =
            {
                FontFamily = new[] { "Open Sans", "sans-serif" },
            }
        },
        Palette =
        {
            AppbarBackground = Colors.Shades.White,
            Background = Colors.Grey.Lighten5
        },
        PaletteDark =
        {
            AppbarBackground = Colors.Grey.Darken3
        },
    };
}