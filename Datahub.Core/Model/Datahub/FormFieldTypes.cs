using System.Collections.Generic;

namespace Datahub.Core.EFCore
{
    public static class FormFieldTypeReference
    {
        public static readonly Dictionary<string, string> EFTypes = new Dictionary<string, string>()
        {
            { "Text", "string" },
            { "Integer", "int" },
            { "Decimal", "double" },
            { "Boolean", "bool" },
            { "Dropdown", "string" },
            { "Date", "DateTime" },
            { "Time", "DateTime" },
            { "Money", "double" }
        };

        public static readonly Dictionary<string, string> Annotations = new Dictionary<string, string>()
        {
            { "Money", "[Column(TypeName=\"Money\")]" }
        };
    }

}