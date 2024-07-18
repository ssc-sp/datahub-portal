using MudBlazor;

namespace Datahub.Core.Data;

public class Icon
{
    public string Name { get; init; }
    public string Color { get; init; }
    public string HexColor { get; init; }

    public static readonly string DEFAULT_PROJECT_ICON = "swatchbook";

    public static readonly Icon HOME = new()
    {
        Name = "fad fa-home",
        Color = "blue",
        HexColor = Colors.Blue.Default,
    };

    public static readonly Icon PROFILE = new()
    {
        //Name = "fad fa-user-astronaut",
        Name = "fad fa-user-helmet-safety",
        Color = "pink",
        HexColor = Colors.Pink.Default,
    };

    public static readonly Icon STORAGE = new()
    {
        Name = "fad fa-hdd",
        Color = "indigo",
        HexColor = Colors.Indigo.Default,
    };

    public static readonly Icon RESOURCES = new()
    {
        Name = "fad fa-sitemap",
        Color = "purple",
        HexColor = Colors.DeepPurple.Default,
    };

    public static readonly Icon TOOLS = new()
    {
        Name = "fad fa-tools",
        Color = "orange",
        HexColor = Colors.Orange.Default,
    };

    public static readonly Icon CATALOG = new()
    {
        Name = "fad fa-books",
        Color = "grey",
    };

    public static readonly Icon DATASETS = new()
    {
        Name = "fad fa-cabinet-filing",
        Color = "pink",
        HexColor = Colors.Pink.Default,
    };

    public static readonly Icon POWERBI = new()
    {
        Name = "fad fa-chart-bar",
        Color = "yellow",
        HexColor = Colors.Yellow.Default,
    };

    public static readonly Icon ADMIN = new()
    {
        Name = "fad fa-user-cog",
        Color = "green",
        HexColor = Colors.Green.Default,
    };

    public static readonly Icon DATAENTRY = new()
    {
        Name = "fad fa-keyboard",
        Color = "grey",
        HexColor = Colors.Gray.Default,
    };

    public static readonly Icon PROJECT = new()
    {
        Name = "fad fa-compass",
        Color = "yellow",
        HexColor = Colors.Green.Darken1,
    };

    public static readonly Icon RESOURCE_ARTICLE = new()
    {
        Name = "fad fa-book",
        Color = "purple",
        HexColor = Colors.Purple.Default,
    };
    public static readonly Icon NEWS = new()
    {
        Name = "fad fa-newspaper",
        Color = "grey",
        HexColor = Colors.Teal.Default,
    };
}