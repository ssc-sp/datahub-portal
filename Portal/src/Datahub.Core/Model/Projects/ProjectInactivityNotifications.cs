namespace Datahub.Core.Model.Projects
{
    /// <summary>
    /// Represents a record of a notification sent due to workspace inactivity.
    /// </summary>
    public class ProjectInactivityNotifications
    {
        /// <summary>
        /// Gets or sets the unique identifier of the workspace that received the inactivity notification.
        /// This is a foreign key to the <see cref="Datahub_Project"/> table.
        /// </summary>
        public int Project_ID { get; set; }

        /// <summary>
        /// Gets or sets the navigation property for the workspace associated with this inactivity notification.
        /// </summary>
        public Datahub_Project Project { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the inactivity notification was sent.
        /// </summary>
        public DateTime NotificationDate { get; set; }

        /// <summary>
        /// Gets or sets the number of days remaining before the workspace is scheduled for deletion,
        /// as indicated at the time the notification was sent.
        /// </summary>
        public int DaysBeforeDeletion { get; set; }

        /// <summary>
        /// Gets or sets a string indicating the recipient(s) of the inactivity notification.
        /// This could be an email address or a list of recipients.
        /// </summary>
        public string SentTo { get; set; }
    }
}