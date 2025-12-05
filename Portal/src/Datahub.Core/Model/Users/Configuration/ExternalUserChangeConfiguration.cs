using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Users.Configuration;

internal class ExternalUserChangeConfiguration : IEntityTypeConfiguration<ExternalUserChange>
{
 public void Configure(EntityTypeBuilder<ExternalUserChange> builder)
 {
 builder.ToTable("ExternalUserChanges");
 builder.HasKey(x => x.Id);
 builder.Property(x => x.Field).HasMaxLength(64);
 builder.Property(x => x.ChangeType).HasMaxLength(64);
 builder.Property(x => x.OldValue).HasMaxLength(256);
 builder.Property(x => x.NewValue).HasMaxLength(256);
 builder.Property(x => x.Reason).HasMaxLength(512);

 builder.HasOne(x => x.ExternalUser)
 .WithMany() // no navigation on ExternalUser to keep coupling minimal
 .HasForeignKey(x => x.ExternalUserId)
 .OnDelete(DeleteBehavior.Cascade);

 builder.HasOne(x => x.ChangedBy)
 .WithMany()
 .HasForeignKey(x => x.ChangedById)
 .OnDelete(DeleteBehavior.NoAction);

 builder.HasOne(x => x.Project)
 .WithMany()
 .HasForeignKey(x => x.ProjectId)
 .OnDelete(DeleteBehavior.NoAction);
 }
}
