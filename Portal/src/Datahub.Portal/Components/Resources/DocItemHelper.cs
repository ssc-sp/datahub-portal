using Datahub.Markdown.Model;
using MudBlazor;
using System.Reflection;

namespace Datahub.Portal.Components.Resources
{
	public static class DocItemHelper
    {
        public static string? GetSidebarIcon(this DocItem item)
        {
            if (item is null)
                return null;
            return item.DocType switch
            {
                DocItemType.Root => @Icons.Material.Outlined.Home,
                DocItemType.Tutorial => @Icons.Material.Outlined.OndemandVideo,
                DocItemType.Folder => null, //@Icons.Material.Outlined.Folder,
                DocItemType.External => @Icons.Material.Outlined.Outbound,
                _ => @Icons.Material.Outlined.Article,
            };
        }

        public static string? GetMudblazorMaterialIcon(this DocItem item, string style)
        {
            if (item.CustomIcon is null) return null;
            return GetMudblazorMaterialIcon(style, item.CustomIcon);
        }

        public static string? GetMudblazorMaterialIcon(string style, string icon)
        {
            //Mudblazor.Icons.Outlined.LibraryBooks;
            var material = typeof(MudBlazor.Icons.Material);
            var nested = material.GetNestedType(style);
            if (nested is null) return null;
            FieldInfo field = nested.GetField(icon); // Get the FieldInfo for the constant

            if (field != null && field.IsLiteral && !field.IsInitOnly)
            {
                return field.GetValue(null) as string; // Access the constant value
            }
            return null;
        }
    }
}
