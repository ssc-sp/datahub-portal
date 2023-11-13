using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Datahub.Stories.Utils;

/// <summary>
/// This class is used to provide default values for the stories.
/// </summary>
public class PlaceholderService
{
    
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContextFactory"></param>
    public PlaceholderService(IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    /// Get a random project from the database
    /// </summary>
    /// <returns></returns>
    public async Task<Datahub_Project> GetRandomProjectAsync()
    {
        // get a random project from the database
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var numberOfProjects = await dbContext.Projects.CountAsync();
        
        var randomProjectIndex = Random.Shared.Next(0, numberOfProjects);
        
        var project = await dbContext.Projects
            .AsNoTracking()
            .Skip(randomProjectIndex)
            .Take(1)
            .FirstAsync();
        return project;
    }
}