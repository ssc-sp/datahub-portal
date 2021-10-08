using System;

namespace Datahub.Shared.Data.External.OpenData
{
    public class OpenDataOrganization
    {
        public Guid Id { get; set; }
        public Guid Revision_Id { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string Approval_Status { get; set; }
        public DateTime Created { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public string Image_Url { get; set; }
        public bool Is_Organization { get; set; }
        public string Type { get; set; }
    }
}