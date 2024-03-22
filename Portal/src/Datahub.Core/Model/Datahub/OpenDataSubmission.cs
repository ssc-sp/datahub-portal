using System.ComponentModel.DataAnnotations.Schema;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Projects;

namespace Datahub.Core.Model.Datahub
{
    public enum OpenDataPublishProcessType
    {
        TbsOpenGovPublishing = 1,
    }

    public record OpenDataSubmissionBasicInfo(string DatasetTitle, OpenDataPublishProcessType ProcessType, int ProjectId);

    public abstract class OpenDataSubmission
    {
        public long Id { get; set; }
        public string UniqueId { get; set; }
        public OpenDataPublishProcessType ProcessType { get; set; }
        public string DatasetTitle { get; set; }
        public string Status { get; set; }
        public bool OpenForAttachingFiles { get; set; }
        public int RequestingUserId { get; set; }
        public DateTime RequestDate { get; set; }
        public int ProjectId { get; set; }

        [NotMapped]
        public abstract string LocalizationPrefix { get; }

        public IList<OpenDataPublishFile> Files { get; set; }

        public Datahub_Project Project { get; set; }
        public PortalUser RequestingUser { get; set; }
    }
}
