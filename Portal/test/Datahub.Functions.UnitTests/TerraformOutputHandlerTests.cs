using System.Text.Json;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Utils;
using Datahub.ProjectTools.Services;
using Datahub.Shared;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Datahub.Functions.UnitTests;

public class TerraformOutputHandlerTests
{
    
    private DatahubProjectDBContext _context;
    
    private TerraformOutputHandler _terraformOutputHandler;
    
    [SetUp]
    public void Setup()
    {
           // setup the DatahubProjectDBContext in a in-memory database for testing
           var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
               .UseInMemoryDatabase(databaseName: "DatahubProjectDBContext")
               .Options;
           
           _context = new DatahubProjectDBContext(options);
           
           _context.Database.EnsureDeleted();
           _context.Database.EnsureCreated();
           
           var mockLogger = new Mock<ILoggerFactory>();
           
           _terraformOutputHandler = new TerraformOutputHandler(mockLogger.Object, _context, null, null, null);
    }
    
    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }


    [Test]
    public async Task ShouldProcessAzureStorageBlobOutputVariables()
    {
        var project = new Datahub_Project();
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        
        var currentPortalUser = new PortalUser
        {
            Email = "tst",
            GraphGuid = Guid.NewGuid().ToString(),
        };
        _context.PortalUsers.Add(currentPortalUser);
        await _context.SaveChangesAsync();

        var storageResource = new Project_Resources2
        {
            Project = project,
            RequestedBy = currentPortalUser,
            ResourceType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob)
        };
        
        _context.Project_Resources2.Add(storageResource);
        await _context.SaveChangesAsync();

        
        var terraformOutput = TerraformOutputHelper.GetExpectedTerraformOutput(project);

        // pass the terraform output to the output variables
        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        var outputVariables = JsonSerializer.Deserialize<Dictionary<string, TerraformOutputVariable>>(terraformOutput, deserializeOptions);
        await _terraformOutputHandler.ProcessAzureStorageBlob(outputVariables!);
        
        var processedResource = await _context.Project_Resources2.FirstOrDefaultAsync(r => r.Project == project);
        var processedResourceJsonContent = JsonSerializer.Deserialize<Dictionary<string, string>>(processedResource!.JsonContent, deserializeOptions);
        
        var accountName = outputVariables![TerraformVariables.OutputAzureStorageAccountName];
        var containerName = outputVariables[TerraformVariables.OutputAzureStorageContainerName];
        var resourceGroupName = outputVariables[TerraformVariables.OutputAzureResourceGroupName];
        
        Assert.That(processedResource, Is.Not.Null);
        Assert.That(processedResource.CreatedAt, Is.Not.Null);
        Assert.That(processedResource.CreatedAt, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-10)));
        
        Assert.That(processedResourceJsonContent, Is.Not.Null);
        Assert.That(processedResourceJsonContent!["storage_account_name"], Is.EqualTo(accountName.Value));
        Assert.That(processedResourceJsonContent!["container_name"], Is.EqualTo(containerName.Value));
        Assert.That(processedResourceJsonContent!["resource_group_name"], Is.EqualTo(resourceGroupName.Value));
        Assert.That(processedResourceJsonContent!["storage_type"], Is.EqualTo(TerraformVariables.AzureStorageType));

    }
}