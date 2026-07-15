using Datahub.Shared.Entities;

namespace Datahub.Core.Data
{
    public class PortalFileMetadata : FileMetadata
    {
        public Microsoft.AspNetCore.Components.Forms.IBrowserFile? BrowserFile { get; set; } = null;
    }
}
