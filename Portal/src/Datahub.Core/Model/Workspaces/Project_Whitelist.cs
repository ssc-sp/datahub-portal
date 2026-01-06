namespace Datahub.Core.Model.Projects
{
    /// <summary>
    /// Represents a whitelist configuration for a given workspace.
    /// </summary>
    public class Project_Whitelist
    {
        /// <summary>
        /// Gets or sets the unique identifier of this corresponding whitelist record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the workspace identifier associated with this record.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the workspace linked to this whitelist record.
        /// </summary>
        public virtual Datahub_Project? Project { get; set; }

        /// <summary>
        /// Gets or sets the user ID of the admin who last updated this record.
        /// </summary>
        public string? AdminLastUpdated_ID { get; set; }

        /// <summary>
        /// Gets or sets the username of the admin who last updated this record.
        /// </summary>
        public string? AdminLastUpdated_UserName { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this record was last updated.
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether storage is allowed for this workspace.
        /// </summary>
        public bool AllowStorage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Databricks is allowed for this workspace.
        /// </summary>
        public bool AllowDatabricks { get; set; }

        /// <summary>
        /// Determines whether the specified workspace whitelist matches this instance.
        /// </summary>
        /// <param name="other">The workspace whitelist to compare with this instance.</param>
        /// <returns>True if the specified whitelist is the same as this one; otherwise, false.</returns>
        public bool IsEqual(Project_Whitelist? other)
        {
            return other != null &&
                   ProjectId == other.ProjectId &&
                   AllowStorage == other.AllowStorage &&
                   AllowDatabricks == other.AllowDatabricks;
        }
    }
}
