using Microsoft.EntityFrameworkCore;
namespace Datahub.Core.EFCore
{
    public class EFCoreDatahubContext : DbContext
    {
        public EFCoreDatahubContext(DbContextOptions<EFCoreDatahubContext> options) : base(options)
        { }
        public DbSet<UserSettings> UserSettings { get; set; }

        public DbSet<UserRecent> UserRecent { get; set; }

        public DbSet<UserRecentLink> UserRecentLinks { get; set; }

    }
}