using Datahub.Core;
using MudBlazor;
using MudBlazor.Utilities;

namespace Datahub.Portal.Layout;

public abstract class DatahubTheme
{

    public const string SideBarContentName = "side-bar";
    public static readonly string SideBorderStyle = new StyleBuilder()
        .AddStyle("border-color", "var(--mud-palette-divider)")
        .AddStyle("border-width", "1px")
        .AddStyle("border-style", "none solid none none")
        .Build();
    
    public static readonly MudTheme DefaultTheme = new()
    {
        LayoutProperties =
        {
            AppbarHeight = "80px"
        },
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
            Body1 =
            {
                FontSize = "0.875rem",
                LineHeight = 1.43,
                LetterSpacing = ".01071em",
            },
            Body2 =
            {
                FontSize = "0.775rem",
                LineHeight = 1.36,
            }
        },
        PaletteLight =
        {
            AppbarBackground = Colors.Shades.White,
            Background = Colors.Gray.Lighten5
        },
        PaletteDark =
        {
            AppbarBackground = Colors.Gray.Darken3
        },
    };
}