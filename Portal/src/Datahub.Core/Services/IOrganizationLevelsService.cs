namespace Datahub.Core.Services;

public interface IOrganizationLevelsService
{
    Task<List<DHOrganizationLevel>> GetBranches();
    Task<List<DHOrganizationLevel>> GetDivisions();
    Task<List<DHOrganizationLevel>> GetSectors();
    Task<List<DHOrganizationLevel>> GetSections();

    Task<DHOrganizationLevel> GetSector(int sectorId);
    Task<DHOrganizationLevel> GetBranch(int branchId);
}

public record DHOrganizationLevel(int Id, int ParentId, string EnglishLabel, string FrenchLabel, string EnglishAcronym, string FrenchAcronym);