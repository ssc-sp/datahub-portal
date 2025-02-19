
using Datahub.Core.Model.Projects;

using Datahub.Core.Components.Resources;

namespace Datahub.Application.Services
{
    public interface IProjectDeletionService
    {
        public Task<bool> DeleteWorkspace(string acronym, Project_Delete_Questionnaire questionnaire);        
        public Task<bool> CleanWorkspaceFromRecentLinks(string workspaceAcronym);
        public Task<bool> CleanResourceFromRecentLinks(string section, string workspaceAcronym);
    }
}