using Microsoft.EntityFrameworkCore;
using Datahub.Core.Model.Datahub;

namespace Datahub.Core.Services;

public class OrganizationLevelsService : IOrganizationLevelsService
{
    private const int SECTOR_ORG_LEVEL = 3;
    private const int BRANCH_ORG_LEVEL = 4;
    private const int DIVISION_ORG_LEVEL = 5;
    private const int SECTION_ORG_LEVEL = 6;

    private readonly IDbContextFactory<DatahubProjectDBContext> dbContextFactory;

    public OrganizationLevelsService(IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public Task<List<DHOrganizationLevel>> GetSectors() => GetLevelChoices(SECTOR_ORG_LEVEL);

    public Task<List<DHOrganizationLevel>> GetBranches() => GetLevelChoices(BRANCH_ORG_LEVEL);

    public Task<List<DHOrganizationLevel>> GetDivisions() => GetLevelChoices(DIVISION_ORG_LEVEL);

    public Task<List<DHOrganizationLevel>> GetSections() => GetLevelChoices(SECTION_ORG_LEVEL);

    private async Task<List<DHOrganizationLevel>> GetLevelChoices(string level)
    {
        using var ctx = dbContextFactory.CreateDbContext();
        return await ctx.OrganizationLevels
            .Where(l => l.OrgLevel == level)
            .Select(l => new DHOrganizationLevel(l.OrganizationID, l.SuperiorOrgId ?? 0, l.OrgNameE, l.OrgNameF, l.OrgAcronymE, l.OrgAcronymF))
            .ToListAsync();
    }

    private async Task<DHOrganizationLevel> GetOrganizationLevel(int id, string level)
    {
        using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.OrganizationLevels
            .Where(l => l.OrganizationID == id && l.OrgLevel == level)
            .Select(l => new DHOrganizationLevel(l.OrganizationID, l.SuperiorOrgId ?? 0, l.OrgNameE, l.OrgNameF, l.OrgAcronymE, l.OrgAcronymF))
            .FirstOrDefaultAsync();
    }

    private async Task<List<DHOrganizationLevel>> GetLevelChoices(int level) => await GetLevelChoices(level.ToString());

    public Task<DHOrganizationLevel> GetSector(int sectorId) => GetOrganizationLevel(sectorId, SECTOR_ORG_LEVEL.ToString());

    public Task<DHOrganizationLevel> GetBranch(int branchId) => GetOrganizationLevel(branchId, BRANCH_ORG_LEVEL.ToString());
}