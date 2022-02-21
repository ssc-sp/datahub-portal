using Datahub.Metadata.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Datahub.Metadata.Utils
{
    public static class CatalogObjectExtensions
    {
        public static async Task<List<CatalogObject>> QueryCatalog(this MetadataDbContext ctx, string query)
        {
            return await ctx.CatalogObjects.FromSqlRaw(query).ToListAsync();
        }
    }
}
