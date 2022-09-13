using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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

        var ruleExpressionsComparer = new ValueComparer<List<string>>(
            (c1, c2) => (c1 == c2) || (c2 != null && c1 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());
            
        
        modelBuilder.Entity<UserObject>()
            .OwnsMany(u => u.UserAchievements)
            .OwnsOne(a => a.Achievement)
            .Property(a => a.RuleExpressions)
            .HasConversion(v => string.Join(';', v!),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList())
            .Metadata.SetValueComparer(ruleExpressionsComparer);
    }
}