using System.Collections.Generic;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public interface IOrganizationLevelsService
    {
        Task<List<OrganizationLevel>> GetBranches();
        Task<List<OrganizationLevel>> GetDivisions();
        Task<List<OrganizationLevel>> GetSectors();
        Task<List<OrganizationLevel>> GetSections();
    }

    public record OrganizationLevel(int Id, int ParentId, string EnglishLabel, string FrenchLabel);
}