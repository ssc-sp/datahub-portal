using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Datahub.Core.Model.Projects;

namespace Datahub.Core.Model.Users.Configuration;

public class UserRoleLinksConfiguration : IEntityTypeConfiguration<UserRoleLinks>
{
    public void Configure(EntityTypeBuilder<UserRoleLinks> builder)
    {
        builder.ToTable("Project_Users");

        builder.HasKey(e => e.ProjectUser_ID);

        builder.HasOne(e => e.Project)
            .WithMany(e => e.UserRoles)
            .HasForeignKey(e => e.Project_ID)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(e => e.Approved_DT);
        builder.HasKey(x => x.ProjectUser_ID);

        builder.Property(x => x.Timestamp)
            .IsRowVersion();

        builder.HasOne(x => x.PortalUser)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(x => x.PortalUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ApprovedPortalUser)
            .WithMany()
            .HasForeignKey(x => x.ApprovedPortalUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
