using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Projects.Configuration
{
    public class ProjectReportsConfiguration : IEntityTypeConfiguration<ProjectReports>
    {
        public void Configure(EntityTypeBuilder<ProjectReports> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).ValueGeneratedOnAdd().IsRequired();
            builder.Property(r => r.ProjectId).IsRequired();
            builder.Property(r => r.GeneratedDate).ValueGeneratedOnAdd();
            builder.Property(r => r.UpdatedDate).ValueGeneratedOnAddOrUpdate();
            builder.Property(r => r.CoverageStartDate).IsRequired();
            builder.Property(r => r.CoverageEndDate).IsRequired();
            builder.Property(r => r.GeneratedBy).HasMaxLength(300).IsRequired();
            builder.Property(r => r.ReportType).IsRequired();
            builder.Property(r => r.ReportStatus).IsRequired();
            builder.Property(r => r.ReportName).HasMaxLength(300).IsRequired();
            builder.Property(r => r.ReportUrl).HasMaxLength(300).IsRequired();
        }
    }
}