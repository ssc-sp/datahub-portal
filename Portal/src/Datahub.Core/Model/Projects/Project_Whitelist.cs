namespace Datahub.Core.Model.Projects
{
    public class Project_Whitelist
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public virtual Datahub_Project Project { get; set; }

        public string AdminLastUpdated_ID { get; set; }
        public string AdminLastUpdated_UserName { get; set; }

        public DateTime LastUpdated { get; set; }

        public bool AllowStorage { get; set; }
        public bool AllowDatabricks { get; set; }
        public bool AllowVMs { get; set; }

        public bool IsEqual(Project_Whitelist other)
        {
            return other != null &&
                   ProjectId == other.ProjectId &&
                   AllowStorage == other.AllowStorage &&
                   AllowDatabricks == other.AllowDatabricks &&
                   AllowVMs == other.AllowVMs;
        }
    }
}