using Microsoft.AspNetCore.Components;

namespace Datahub.Portal.Components.Tables
{
    public class DHTableLinkList : IDHTableCell
    {
        public List<(string Name, string Url)> Links { get; set; } = [];

        public RenderFragment Render()
        {
            return builder =>
            {
                int sequence = 0;
                builder.OpenElement(sequence++, "div"); // Open a container element
                builder.AddAttribute(sequence++, "style",
                    "display: flex; flex-direction: column;"); // Apply vertical stacking
                foreach (var link in Links)
                {
                    builder.OpenComponent<MudBlazor.MudLink>(sequence++);
                    if (!string.IsNullOrWhiteSpace(link.Url))
                    {
                        builder.AddAttribute(sequence++, "Href", link.Url);
                        builder.AddAttribute(sequence++, "Target", "_blank");
                    }

                    builder.AddAttribute(sequence++, "ChildContent",
                        (RenderFragment)(childBuilder => { childBuilder.AddContent(0, link.Name); }));
                    builder.CloseComponent();
                }

                builder.CloseElement(); // Close the container element
            };
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            var names = ToString();
            FormattableString formattable = $"{names}";
            return formattable.ToString(formatProvider);
        }

        public override string ToString()
        {
            return string.Join(", ", Links.Select(l => l.Name));
        }
    }
}