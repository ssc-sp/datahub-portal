using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Datahub.LanguageTraining.Data;

public class LanguageTrainingDBContext : DbContext
{
	public LanguageTrainingDBContext(DbContextOptions<LanguageTrainingDBContext> options) : base(options)
	{
	}

	public DbSet<LanguageTrainingApplication> LanguageTrainingApplications { get; set; } = null!;
	public DbSet<SeasonRegistrationPeriod> SeasonRegistrationPeriods { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<SeasonRegistrationPeriod>()
			.HasIndex(e => new { e.Year_NUM, e.Quarter_NUM })
			.IsUnique();
	}

	public void Seed(LanguageTrainingDBContext context, IConfiguration configuration)
	{
		foreach (var entity in SeedingUtils.GetSeasonRegistrationPeriodSeeding(2022, 100))
		{
			context.SeasonRegistrationPeriods.Add(entity);
		}
	}
}