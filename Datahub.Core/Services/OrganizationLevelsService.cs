using Datahub.Core.EFCore;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Datahub.Core.Services
{
    public class OrganizationLevelsService : IOrganizationLevelsService
    {
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;

        public OrganizationLevelsService(IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public Task<List<OrganizationLevel>> GetSectors() => GetLevelChoices("3");

        public Task<List<OrganizationLevel>> GetBranches() => GetLevelChoices("4");

        public Task<List<OrganizationLevel>> GetDivisions() => GetLevelChoices("5");

        public Task<List<OrganizationLevel>> GetSections() => GetLevelChoices("6");

        public async Task<List<OrganizationLevel>> GetLevelChoices(string level)
        {
            using var ctx = _dbContextFactory.CreateDbContext();
            return await ctx.Organization_Levels
                .Where(l => l.Org_Level == level)
                .Select(l => new OrganizationLevel(l.Organization_ID, l.Superior_OrgId ?? 0, l.Org_Name_E, l.Org_Name_F))
                .ToListAsync();
        }
    }
}