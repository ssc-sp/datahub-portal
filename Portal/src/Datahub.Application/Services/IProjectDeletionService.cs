
using Datahub.Core.Model.Projects;

namespace Datahub.Application.Services
{
    public interface IProjectDeletionService
    {
        public Task<bool> DeleteWorkspace(string acronym, Project_Delete_Questionnaire questionnaire);        
        public Task<bool> CleanWorkspaceFromRecentLinks(string workspaceAcronym);
        public Task<bool> CleanResourceFromRecentLinks(string section, string workspaceAcronym);
    }
}