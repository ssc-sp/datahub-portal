namespace Datahub.Core.Model.Projects
{
    public class ProjectWhitelist
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public virtual DatahubProject Project { get; set; }

        public string AdminLastUpdatedID { get; set; }
        public string AdminLastUpdatedUserName { get; set; }

        public DateTime LastUpdated { get; set; }

        public bool AllowStorage { get; set; }
        public bool AllowDatabricks { get; set; }
        public bool AllowVMs { get; set; }

        public bool IsEqual(ProjectWhitelist other)
        {
            return other != null &&
                   ProjectId == other.ProjectId &&
                   AllowStorage == other.AllowStorage &&
                   AllowDatabricks == other.AllowDatabricks &&
                   AllowVMs == other.AllowVMs;
        }
    }
}