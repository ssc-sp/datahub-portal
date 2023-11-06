using MudBlazor;

namespace Datahub.Portal.Layout;

public abstract class DatahubTheme
{
    public static readonly MudTheme DefaultTheme = new()
    {
        Typography =
        {
            Default =
            {
                FontFamily = new[] { "Open Sans", "sans-serif" },
                FontSize = "0.9rem",        
                LineHeight = 1.75,
            }, 
            H1 = new H1()
            {
                LineHeight = 1.25,
                FontSize = "2.5rem",
                FontWeight = 600,
            },
            H2 = new H2()
            {
                LineHeight = 1.35,
                FontSize = "1.75rem",
                FontWeight = 600,
            },
            H3 = new H3()
            {
                LineHeight = 1.3,
                FontSize = "1.5rem",
                FontWeight = 600,
            },
            H4 = new H4()
            {
                LineHeight = 1.2,
                FontSize = "1.25rem",
                FontWeight = 600,
            },
            H5 = new H5()
            {
                LineHeight = 1.2,
                FontSize = "1.125rem",
                FontWeight = 600,
            },
            H6 = new H6()
            {
                LineHeight = 1.2,
                FontSize = "1rem",
                FontWeight = 600,
            },
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