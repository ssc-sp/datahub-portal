using Datahub.Core.Configuration;
using Datahub.Core.Model.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Users.Configuration
{
    internal class WorkspaceInvitationConfiguration : IEntityTypeConfiguration<WorkspaceInvitation>
    {
        public void Configure(EntityTypeBuilder<WorkspaceInvitation> builder)
        {
            builder.ToTable("WorkspaceInvitations");

            // Key
            builder.HasKey(i => i.RequestID);
            builder.Property(i => i.RequestID)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.InvitedEmail)
                .IsRequired()
                .HasMaxLength(ConfigurationConstants.EMAIL_MAX_LENGTH);

            builder.Property(i => i.InvitationToken)
                .IsRequired();

            builder.HasIndex(i => i.InvitationToken)
                .IsUnique();

            builder.Property(i => i.Request_DT)
                .HasConversion(
                    v => v,
                    v => v)
                .HasColumnType("datetimeoffset");

            // Relationships
            builder.HasOne(i => i.Project)
                .WithMany() // avoid relying on a project-side navigation
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
