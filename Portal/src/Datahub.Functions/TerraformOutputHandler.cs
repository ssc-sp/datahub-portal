using System.Text.Json;
using Datahub.Core.Enums;
using Datahub.Core.Model.Datahub;
using DefaultNamespace;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions;

public class TerraformOutputHandler
{
    private readonly DatahubProjectDBContext _projectDbContext;
    private readonly ILogger _logger;

    private const string NewProjectTemplate = "new_project_template";
    private const string ProjectAcronym = "project_cd";

    public TerraformOutputHandler(ILoggerFactory loggerFactory, DatahubProjectDBContext projectDbContext)
    {
        _projectDbContext = projectDbContext;
        _logger = loggerFactory.CreateLogger("TerraformOutputHandler");
    }

    [Function("TerraformOutputHandler")]
    public async Task RunAsync(
        [QueueTrigger("terraform-output", Connection = "TerraformOutputConnectionString")]
        string myQueueItem,
        FunctionContext context)
    {
        _logger.LogInformation($"C# Queue trigger function started");

        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var output =
            JsonSerializer.Deserialize<Dictionary<string, TerraformOutputVariable>>(myQueueItem, deserializeOptions);

        _logger.LogInformation("C# Queue trigger function processing: {OutputCount} items", output?.Count);

        if (output is null)
        {
            throw new ArgumentNullException(nameof(output));
        }
        
        try
        {
            await ProcessTerraformOutputVariables(output);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing output variable {OutputVariable}", output);
            throw;
        }

        _logger.LogInformation($"C# Queue trigger function finished");
    }

    private async Task ProcessTerraformOutputVariables(
        IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
    {
        await ProcessProjectStatus(outputVariables);
    }

    private async Task ProcessProjectStatus(IReadOnlyDictionary<string, TerraformOutputVariable> outputVariables)
    {
        var projectAcronym = outputVariables[ProjectAcronym];
        var project = await _projectDbContext.Projects
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym.Value);
        
        if (project == null)
        {
            _logger.LogError("Project not found for acronym {ProjectId}", projectAcronym.Value);
            throw new Exception($"Project not found for acronym {projectAcronym.Value}");
        }
        
        var outputPhase = GetProjectPhaseMapping(outputVariables[NewProjectTemplate].Value); 
        if(project.Project_Phase != outputPhase)
        {
            project.Project_Phase = outputPhase;
            await _projectDbContext.SaveChangesAsync();
        }
    }

    private static string GetProjectPhaseMapping(string value)
    {
        return value switch
        {
            "completed" => ProjectPhase.Completed,
            "in_progress" => ProjectPhase.InProgress,
            _ => ProjectPhase.PendingApproval
        };
    }
}