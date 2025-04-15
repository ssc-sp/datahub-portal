using Microsoft.AspNetCore.Components;

namespace Datahub.Portal.Pages.Tools.Statistics.Objects
{
    public interface IDHTableCell : IFormattable
    {
        public RenderFragment Render();
    }
}