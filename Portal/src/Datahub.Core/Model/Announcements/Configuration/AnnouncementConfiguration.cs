using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Datahub.Core.Model.Announcements.Configuration;

internal class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.ToTable("Announcements");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.TitleEn)
               .IsRequired()
               .HasMaxLength(128);

        builder.Property(e => e.TitleFr)
               .IsRequired()
               .HasMaxLength(128);

        builder.Property(e => e.PreviewEn)
               .IsRequired();

        builder.Property(e => e.PreviewFr)
               .IsRequired();

        builder.Property(e => e.BodyEn)
               .IsRequired();

        builder.Property(e => e.BodyFr)
               .IsRequired();
    }
}
