namespace Datahub.Application.Services.Notebooks;

public interface IDatabricksNotebookService
{
        public Task<List<string>> ListAllNotebooksAsync();
        
}