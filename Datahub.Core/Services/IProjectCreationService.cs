using System.Collections.Generic;
using System.Threading.Tasks;
using Datahub.Core.EFCore;

namespace Datahub.Core.Services;

public interface IProjectCreationService
{
    public Task<string> GenerateProjectAcronymAsync(string projectName);
    public Task<string> GenerateProjectAcronymAsync(string projectName, IEnumerable<string> existingAcronyms);
    public Task CreateProjectAsync(string projectName, string acronym, string organization); 
    public Task CreateProjectAsync(string projectName, string organization); 
}