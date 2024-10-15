using Microsoft.EntityFrameworkCore;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Context;

namespace Datahub.Core.Services;

public class OrganizationLevelsService : IOrganizationLevelsService
{
    private const int SECTOR_ORG_LEVEL = 3;
    private const int BRANCH_ORG_LEVEL = 4;
    private const int DIVISION_ORG_LEVEL = 5;
    private const int SECTION_ORG_LEVEL = 6;

    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;

    public OrganizationLevelsService(IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public Task<List<OrganizationLevel>> GetSectors() => GetLevelChoices(SECTOR_ORG_LEVEL);

    public Task<List<OrganizationLevel>> GetBranches() => GetLevelChoices(BRANCH_ORG_LEVEL);

    public Task<List<OrganizationLevel>> GetDivisions() => GetLevelChoices(DIVISION_ORG_LEVEL);

    public Task<List<OrganizationLevel>> GetSections() => GetLevelChoices(SECTION_ORG_LEVEL);

    private async Task<List<OrganizationLevel>> GetLevelChoices(string level)
    {
        using var ctx = _dbContextFactory.CreateDbContext();
        return await ctx.Organization_Levels
            .Where(l => l.Org_Level == level)
            .Select(l => new OrganizationLevel(l.Organization_ID, l.Superior_OrgId ?? 0, l.Org_Name_E, l.Org_Name_F, l.Org_Acronym_E, l.Org_Acronym_F))
            .ToListAsync();
    }

    private async Task<OrganizationLevel> GetOrganizationLevel(int id, string level)
    {
        using var ctx = await _dbContextFactory.CreateDbContextAsync();
        return await ctx.Organization_Levels
            .Where(l => l.Organization_ID == id && l.Org_Level == level)
            .Select(l => new OrganizationLevel(l.Organization_ID, l.Superior_OrgId ?? 0, l.Org_Name_E, l.Org_Name_F, l.Org_Acronym_E, l.Org_Acronym_F))
            .FirstOrDefaultAsync();
    }

    private async Task<List<OrganizationLevel>> GetLevelChoices(int level) => await GetLevelChoices(level.ToString());

    public Task<OrganizationLevel> GetSector(int sectorId) => GetOrganizationLevel(sectorId, SECTOR_ORG_LEVEL.ToString());

    public Task<OrganizationLevel> GetBranch(int branchId) => GetOrganizationLevel(branchId, BRANCH_ORG_LEVEL.ToString());
}