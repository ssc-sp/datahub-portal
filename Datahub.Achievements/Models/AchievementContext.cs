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
            .OwnsOne(u => u.Telemetry)
            .OwnsMany(m => m.EventMetrics);
        modelBuilder.Entity<UserObject>()
            .OwnsMany(u => u.UserAchievements)
            .OwnsOne(a => a.Achievement)
            .Property(a => a.RuleExpressions)
            .HasConversion(v => string.Join(',', v!),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
    }
}