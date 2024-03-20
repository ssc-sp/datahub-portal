namespace Datahub.Core.Services;

public interface IOrganizationLevelsService
{
    Task<List<OrganizationLevel>> GetBranches();
    Task<List<OrganizationLevel>> GetDivisions();
    Task<List<OrganizationLevel>> GetSectors();
    Task<List<OrganizationLevel>> GetSections();

    Task<OrganizationLevel> GetSector(int sectorId);
    Task<OrganizationLevel> GetBranch(int branchId);
}

public record OrganizationLevel(int Id, int ParentId, string EnglishLabel, string FrenchLabel, string EnglishAcronym, string FrenchAcronym);