using Datahub.Core.Services;

public class OrganizationLevelsService : IOrganizationLevelsService
{
    public Task<List<OrganizationLevel>> GetBranches() => Task.FromResult(new List<OrganizationLevel>());
    public Task<List<OrganizationLevel>> GetDivisions() => Task.FromResult(new List<OrganizationLevel>());
    public Task<List<OrganizationLevel>> GetSectors() => Task.FromResult(new List<OrganizationLevel>());
    public Task<List<OrganizationLevel>> GetSections() => Task.FromResult(new List<OrganizationLevel>());
    public Task<OrganizationLevel> GetSector(int sectorId) => Task.FromResult<OrganizationLevel>(null);
    public Task<OrganizationLevel> GetBranch(int branchId) => Task.FromResult<OrganizationLevel>(null);
}
