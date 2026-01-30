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
            AppbarHeight = "70px"
        },
        Typography =
        {
            Default =
            {
                FontFamily = new[] { "Open Sans", "sans-serif" },
                FontSize = "0.9rem",        
                LineHeight = "1.75",
            }, 
            H1 = new H1Typography()
            {
                LineHeight = "1.25",
                FontSize = "2.5rem",
                FontWeight = "600",
            },
            H2 = new H2Typography()
            {
                LineHeight = "1.35",
                FontSize = "1.75rem",
                FontWeight = "600",
            },
            H3 = new H3Typography()
            {
                LineHeight = "1.3",
                FontSize = "1.5rem",
                FontWeight = "600",
            },
            H4 = new H4Typography()
            {
                LineHeight = "1.2",
                FontSize = "1.25rem",
                FontWeight = "600",
            },
            H5 = new H5Typography()
            {
                LineHeight = "1.2",
                FontSize = "1.125rem",
                FontWeight = "600",
            },
            H6 = new H6Typography()
            {
                LineHeight = "1.2",
                FontSize = "1rem",
                FontWeight = "600",
            },
            Body1 =
            {
                FontSize = "0.875rem",
                LineHeight = "1.43",
                LetterSpacing = ".01071em",
            },
            Body2 =
            {
                FontSize = "0.775rem",
                LineHeight = "1.36",
            }
        },
        PaletteLight =
        {
            //The primary accent colour (becomes close to white in dark mode)
            Primary = "#2460FF",
            PrimaryDarken = "#26374A",

            //Used for Unclassified data labels (same colour as Primary but does not change in dark mode)
            Info = "#2460FF",

            //Used for Protected A data labels
            Secondary = "#0535d2",
            SecondaryDarken = "#042CAE",
            SecondaryLighten = "#7897FC",

            //Used for Protected B data labels
            Tertiary = "#2b4380",
            TertiaryDarken = "#284162",
            TertiaryLighten = "#6584A6",

            //Darkened yellow to meet WCAG standards
            Warning = "#B3800F",
            WarningLighten = "#FAEDD1",


            //Darkened red (colours are GC standard --gcds-color-red 500, 900, 100)
            Error = "#d3080c",
            ErrorDarken = "#822117",
            ErrorLighten = "#FBDDDA",

            AppbarBackground = Colors.Shades.White,
            Background = Colors.Gray.Lighten5
        },
        PaletteDark =
        {
            Primary = "#C8C4F3",

            //Used for Unclassified data labels (same colour as Primary but does not change in dark mode)
            Info = "#2460FF",

            //Used for Protected A data labels
            Secondary = "#0535d2",
            SecondaryDarken = "#042CAE",
            SecondaryLighten = "#7897FC",

            //Used for Protected B data labels
            Tertiary = "#2b4380",
            TertiaryDarken = "#284162",
            TertiaryLighten = "#6584A6",

            //Darkened yellow to meet WCAG standards
            Warning = "#B3800F",
            WarningLighten = "#FAEDD1",

            //Darkened red (colours are GC standard --gcds-color-red 500, 900, 100)
            Error = "#d3080c",
            ErrorDarken = "#822117",
            ErrorLighten = "#FBDDDA",

            AppbarBackground = "#27272F",
            Background = "#27272F"
        },
    };
}
