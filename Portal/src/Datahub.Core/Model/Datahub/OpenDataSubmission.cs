using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.CloudStorage;
using Datahub.Core.Model.Projects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Model.Datahub
{
    public enum OpenDataPublishProcessType
    {
        TBSOpenGov = 1,
    }

    public abstract class OpenDataSubmission
    {
        public long Id { get; set; }
        public OpenDataPublishProcessType ProcessType { get; set; }
        public string Status { get; set; }
        public int RequestingUserId { get; set; }
        public DateTime RequestDate { get; set; }
        public int ProjectId { get; set; }
        public int? ProjectStorageId { get; set; }

        [NotMapped]
        public abstract string LocalizationPrefix { get; }

        public IList<OpenDataPublishFile> Files { get; set; }

        public Datahub_Project Project { get; set; }
        public ProjectCloudStorage Storage { get; set; }
        public PortalUser RequestingUser { get; set; }
    }
}
