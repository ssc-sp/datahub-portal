using Microsoft.AspNetCore.Components;

namespace Datahub.Portal.Components.Tables
{
    public interface IDHTableCell : IFormattable
    {
        public RenderFragment Render();
    }
}