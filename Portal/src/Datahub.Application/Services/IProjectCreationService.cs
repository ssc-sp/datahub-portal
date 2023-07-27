namespace Datahub.Application.Services;

public interface IProjectCreationService
{
    public Task<bool> AcronymExists(string acronym);
    public Task<string> GenerateProjectAcronymAsync(string projectName);
    public Task<string> GenerateProjectAcronymAsync(string projectName, IEnumerable<string> existingAcronyms);
    //token needs to be acquired by component so that exception handling can be done there
    //(handling exception causes force refresh through navigation manager)
    public Task<bool> CreateProjectAsync(string projectName, string acronym, string organization); 
    public Task<bool> CreateProjectAsync(string projectName, string organization);
    
    public Task SaveProjectCreationDetailsAsync(string projectAcronym, string interestedFeatures);
}