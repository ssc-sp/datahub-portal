namespace Datahub.Core.Model.Users
{
    /// <summary>
    /// Represents a record of user inactivity notifications, including
    /// the date of notification and the days remaining before a user is locked or deleted.
    /// </summary>
    public class UserInactivityNotifications
    {
        /// <summary>
        /// Gets or sets the user's identifier.
        /// </summary>
        public int User_ID { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="PortalUser"/> associated with this record.
        /// </summary>
        public PortalUser User { get; set; } = null!;

        /// <summary>
        /// Gets or sets the date and time the notification was issued.
        /// </summary>
        public DateTime NotificationDate { get; set; }

        /// <summary>
        /// Gets or sets the number of days remaining before the user's account is locked.
        /// </summary>
        public int DaysBeforeLocked { get; set; }

        /// <summary>
        /// Gets or sets the number of days remaining before the user's account is deleted.
        /// </summary>
        public int DaysBeforeDeleted { get; set; }
    }
}
