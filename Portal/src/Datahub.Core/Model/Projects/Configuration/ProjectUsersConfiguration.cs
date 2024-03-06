using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Projects.Configuration;

public class ProjectUsersConfiguration : IEntityTypeConfiguration<DatahubProjectUser>
{
    public void Configure(EntityTypeBuilder<DatahubProjectUser> builder)
    {
        builder.ToTable("Project_Users");

        builder.HasKey(e => e.ProjectUserID);

        // builder.HasOne(e => e.PortalUser)
        //     .WithMany()
        //     .HasForeignKey(e => e.PortalUserId)
        //     .OnDelete(DeleteBehavior.NoAction);

        // builder.HasOne(e => e.ApprovedPortalUser)
        //     .WithMany()
        //     .HasForeignKey(e => e.ApprovedPortalUserId)
        //     .OnDelete(DeleteBehavior.NoAction);

        // builder.HasOne(e => e.Role)
        //     .WithMany()
        //     .HasForeignKey(e => e.RoleId)
        //     .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.Project)
            .WithMany(e => e.Users)
            .HasForeignKey(e => e.ProjectID)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(e => e.ApprovedDT);
    }
}