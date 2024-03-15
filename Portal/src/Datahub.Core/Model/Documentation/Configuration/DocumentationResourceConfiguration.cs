using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datahub.Core.Model.Documentation.Configuration;

internal class DocumentationResourceConfiguration : IEntityTypeConfiguration<DocumentationResource>
{
    public void Configure(EntityTypeBuilder<DocumentationResource> builder)
    {
        builder.ToTable("DocumentationResources");
        builder.HasKey(e => e.Id);
    }
}
