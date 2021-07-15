using Microsoft.EntityFrameworkCore;
namespace NRCan.Datahub.Shared.EFCore
{
    public class EFCoreDatahubContext : DbContext
    {
        public EFCoreDatahubContext(DbContextOptions<EFCoreDatahubContext> options) : base(options)
        { }
        public DbSet<UserSettings> UserSettings { get; set; }

        public DbSet<UserRecent> UserRecent { get; set; }
        
    }
}