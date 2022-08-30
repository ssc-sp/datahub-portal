using Microsoft.EntityFrameworkCore;

namespace Datahub.Achievements.Models;

public class AchievementContext : DbContext
{
    public AchievementContext(DbContextOptions<AchievementContext> options)
        : base(options)
    {
    }
    
    public DbSet<UserObject>? UserObjects { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserObject>()
            .OwnsOne(u => u.Telemetry);
        modelBuilder.Entity<UserObject>()
            .OwnsOne(u => u.UserAchievements);
    }
}