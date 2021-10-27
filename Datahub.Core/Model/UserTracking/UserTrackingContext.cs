using Microsoft.EntityFrameworkCore;
namespace Datahub.Core.UserTracking
{
    public class UserTrackingContext : DbContext
    {
        public UserTrackingContext(DbContextOptions<UserTrackingContext> options) : base(options)
        { }
        public DbSet<UserSettings> UserSettings { get; set; }

        public DbSet<UserRecent> UserRecent { get; set; }

        public DbSet<UserRecentLink> UserRecentLinks { get; set; }

    }
}