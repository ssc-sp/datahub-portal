using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Onboarding.Configuration;

public class SelfRegistrationDetailsConfiguration : IEntityTypeConfiguration<SelfRegistrationDetails>
{
    public void Configure(EntityTypeBuilder<SelfRegistrationDetails> builder)
    {
        builder.ToTable("SelfRegistrationDetails");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.Comment)
            .IsRequired(false);
    }
}