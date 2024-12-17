
namespace Datahub.Application.Services
{
    public interface IProjectDeletionService
    {
        public Task<bool> DeleteWorkspace(string acronym);
    }
}