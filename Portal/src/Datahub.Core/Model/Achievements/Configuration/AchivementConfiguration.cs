using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Achievements.Configuration;

public class AchivementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.HasData(Achievement.GetAll());

        builder.ToTable("Achivements");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
               .HasMaxLength(8);

        builder.Property(e => e.Name)
               .IsRequired()
               .HasMaxLength(128);

        builder.Property(e => e.Description)
               .IsRequired();

        builder.HasMany(e => e.UserAchievements)
               .WithOne(e => e.Achievement)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
