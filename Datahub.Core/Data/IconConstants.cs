using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MudBlazor;

namespace Datahub.Core.Data
{
    public class Icon
    {
        public string Name { get; set; }
        public string Color { get; set; }
        
        public static readonly string DEFAULT_PROJECT_ICON = "swatchbook";

        public static readonly Icon HOME = new()
        {
            Name = "fad fa-home",
            Color = Colors.Blue.Default,
        };

        public static readonly Icon STORAGE = new()
        {
            Name = "fad fa-hdd",
            Color = Colors.Indigo.Default,
        };

        public static readonly Icon RESOURCES = new()
        {
            Name = "fad fa-books",
            Color = Colors.Purple.Default,
        };

        public static readonly Icon TOOLS = new()
        {            
            Name = "fad fa-tools",
            Color = Colors.Orange.Default,
        };

        public static readonly Icon DATASETS = new()
        {
            Name = "fad fa-cabinet-filing",
            Color = Colors.Pink.Default,
        };

        public static readonly Icon POWERBI = new()
        {
            Name = "fad fa-chart-bar",
            Color = Colors.Yellow.Default,
        };

        public static readonly Icon ADMIN = new()
        {
            Name = "fad fa-user-cog",
            Color = Colors.Green.Default,
        };

        public static readonly Icon DATAENTRY = new()
        {
            Name = "fad fa-keyboard",
            Color = Colors.Grey.Default,
        };

        public static readonly Icon PROJECT = new()
        {
            Name = "fad fa-project-diagram",
            Color = Colors.Yellow.Default,
        };
    }
}
