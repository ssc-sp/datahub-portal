using System.Text.Json;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Utils;
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
        var options = new DbContextOptionsBuilder<SqlServerDatahubContext>()
            .UseInMemoryDatabase(databaseName: "DatahubProjectDBContext")
            .Options;

        _context = new SqlServerDatahubContext(options);

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
        var project = new Datahub_Project()
        {
            Project_Acronym_CD = "STORAGE"
        };
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

        var outputVariables =
            JsonSerializer.Deserialize<Dictionary<string, TerraformOutputVariable>>(terraformOutput,
                deserializeOptions);
        await _terraformOutputHandler.ProcessAzureStorageBlob(outputVariables!);

        var processedResource = await _context.Project_Resources2.FirstOrDefaultAsync(r => r.Project == project);

        Assert.That(processedResource, Is.Not.Null);
        Assert.That(processedResource!.CreatedAt, Is.Not.Null);
        Assert.That(processedResource.CreatedAt, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-10)));


        var processedResourceJsonContent =
            JsonSerializer.Deserialize<Dictionary<string, string>>(processedResource!.JsonContent, deserializeOptions);
        var accountName = outputVariables![TerraformVariables.OutputAzureStorageAccountName];
        var containerName = outputVariables[TerraformVariables.OutputAzureStorageContainerName];
        var resourceGroupName = outputVariables[TerraformVariables.OutputAzureResourceGroupName];

        Assert.That(processedResourceJsonContent, Is.Not.Null);
        Assert.That(processedResourceJsonContent!["storage_account"], Is.EqualTo(accountName.Value));
        Assert.That(processedResourceJsonContent!["container"], Is.EqualTo(containerName.Value));
        Assert.That(processedResourceJsonContent!["resource_group_name"], Is.EqualTo(resourceGroupName.Value));
        Assert.That(processedResourceJsonContent!["storage_type"], Is.EqualTo(TerraformVariables.AzureStorageType));

        var processedResourceInputJsonContent =
            JsonSerializer.Deserialize<Dictionary<string, string>>(processedResource!.InputJsonContent,
                deserializeOptions);

        Assert.That(processedResourceInputJsonContent, Is.Not.Null);
        Assert.That(processedResourceInputJsonContent!["storage_type"],
            Is.EqualTo(TerraformVariables.AzureStorageType));
    }


    [Test]
    public async Task ShouldProcessAzureDatabricksOutputVariables()
    {
        var project = new Datahub_Project()
        {
            Project_Acronym_CD = "DATABRICKS"
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var currentPortalUser = new PortalUser
        {
            Email = "tst",
            GraphGuid = Guid.NewGuid().ToString(),
        };
        _context.PortalUsers.Add(currentPortalUser);
        await _context.SaveChangesAsync();

        var databricksResource = new Project_Resources2
        {
            Project = project,
            RequestedBy = currentPortalUser,
            ResourceType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureDatabricks)
        };
        _context.Project_Resources2.Add(databricksResource);
        await _context.SaveChangesAsync();

        // stub in fake databricks workspace id and url
        var fakeWorkspaceId = "fake-databricks-workspace-id";
        var fakeWorkspaceUrl = "fake-databricks-workspace-url";
        var terraformOutput =
            TerraformOutputHelper.GetExpectedTerraformOutput(project, fakeWorkspaceId, fakeWorkspaceUrl);
        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var outputVariables =
            JsonSerializer.Deserialize<Dictionary<string, TerraformOutputVariable>>(terraformOutput,
                deserializeOptions);


        await _terraformOutputHandler.ProcessAzureDatabricks(outputVariables!);

        var processedResource = await _context.Project_Resources2.FirstOrDefaultAsync(r => r.Project == project);

        Assert.That(processedResource, Is.Not.Null);
        Assert.That(processedResource!.CreatedAt, Is.Not.Null);
        Assert.That(processedResource.CreatedAt, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-10)));

        var processedResourceJsonContent =
            JsonSerializer.Deserialize<Dictionary<string, string>>(processedResource!.JsonContent, deserializeOptions);
        var workspaceId = outputVariables![TerraformVariables.OutputAzureDatabricksWorkspaceId];
        var workspaceUrl = outputVariables[TerraformVariables.OutputAzureDatabricksWorkspaceUrl];
        var workspaceName = outputVariables[TerraformVariables.OutputAzureDatabricksWorkspaceName];

        Assert.That(processedResourceJsonContent, Is.Not.Null);
        Assert.That(processedResourceJsonContent!["workspace_id"], Is.EqualTo(workspaceId.Value));
        Assert.That(processedResourceJsonContent!["workspace_url"], Is.EqualTo(workspaceUrl.Value));
        Assert.That(processedResourceJsonContent!["workspace_name"], Is.EqualTo(workspaceName.Value));

        var processedResourceInputJsonContent =
            JsonSerializer.Deserialize<Dictionary<string, string>>(processedResource!.InputJsonContent,
                deserializeOptions);

        Assert.That(processedResourceInputJsonContent, Is.Not.Null);
    }

    [Test]
    public async Task ShouldProcessAzureWebAppOutputVariables()
    {
        var project = new Datahub_Project()
        {
            Project_Acronym_CD = "WEBAPP"
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var currentPortalUser = new PortalUser
        {
            Email = "tst",
            GraphGuid = Guid.NewGuid().ToString(),
        };
        _context.PortalUsers.Add(currentPortalUser);
        await _context.SaveChangesAsync();

        var webAppResource = new Project_Resources2
        {
            Project = project,
            RequestedBy = currentPortalUser,
            ResourceType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureAppService)
        };

        _context.Project_Resources2.Add(webAppResource);
        await _context.SaveChangesAsync();

        var terraformOutput = TerraformOutputHelper.GetExpectedTerraformOutput(project);
        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var outputVariables =
            JsonSerializer.Deserialize<Dictionary<string, TerraformOutputVariable>>(terraformOutput,
                deserializeOptions);

        await _terraformOutputHandler.ProcessAzureWebApp(outputVariables!);

        var processedResource = await _context.Project_Resources2.FirstOrDefaultAsync(r => r.Project == project);

        Assert.That(processedResource, Is.Not.Null);
        Assert.That(processedResource!.CreatedAt, Is.Not.Null);
        Assert.That(processedResource.CreatedAt, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-10)));

        var processedResourceJsonContent =
            JsonSerializer.Deserialize<Dictionary<string, string>>(processedResource!.JsonContent, deserializeOptions);
        var webAppId = outputVariables![TerraformVariables.OutputAzureAppServiceId];
        var webAppHostname = outputVariables[TerraformVariables.OutputAzureAppServiceHostName];

        Assert.That(processedResourceJsonContent, Is.Not.Null);
        Assert.That(processedResourceJsonContent!["app_service_id"], Is.EqualTo(webAppId.Value));
        Assert.That(processedResourceJsonContent!["app_service_hostname"], Is.EqualTo(webAppHostname.Value));

        var processedResourceInputJsonContent =
            JsonSerializer.Deserialize<Dictionary<string, string>>(processedResource!.InputJsonContent,
                deserializeOptions);

        Assert.That(processedResourceInputJsonContent, Is.Not.Null);

        var updatedProject = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == project.Project_Acronym_CD);

        Assert.That(updatedProject, Is.Not.Null);
        Assert.That(updatedProject!.WebApp_URL, Is.EqualTo(webAppHostname.Value));
        Assert.That(updatedProject!.WebAppEnabled, Is.True);
    }

    [Test]
    public async Task ShouldProcessAzurePostgresOutputVariables()
    {
        var project = new Datahub_Project()
        {
            Project_Acronym_CD = "POSTGRES"
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var currentPortalUser = new PortalUser
        {
            Email = "tst",
            GraphGuid = Guid.NewGuid().ToString(),
        };
        _context.PortalUsers.Add(currentPortalUser);
        await _context.SaveChangesAsync();

        var postgresResource = new Project_Resources2
        {
            Project = project,
            RequestedBy = currentPortalUser,
            ResourceType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres)
        };

        _context.Project_Resources2.Add(postgresResource);
        await _context.SaveChangesAsync();

        var terraformOutput = TerraformOutputHelper.GetExpectedTerraformOutput(project);
        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var outputVariables =
            JsonSerializer.Deserialize<Dictionary<string, TerraformOutputVariable>>(terraformOutput,
                deserializeOptions);

        await _terraformOutputHandler.ProcessAzurePostgres(outputVariables!);

        var processedResource = await _context.Project_Resources2.FirstOrDefaultAsync(r => r.Project == project);

        Assert.That(processedResource, Is.Not.Null);
        Assert.That(processedResource!.CreatedAt, Is.Not.Null);
        Assert.That(processedResource.CreatedAt, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-10)));

        var processedResourceJsonContent =
            JsonSerializer.Deserialize<Dictionary<string, string>>(processedResource!.JsonContent, deserializeOptions);
        var postgresDatabaseName = outputVariables![TerraformVariables.OutputAzurePostgresDatabaseName];
        var postgresDns = outputVariables[TerraformVariables.OutputAzurePostgresDns];
        var postgresId = outputVariables[TerraformVariables.OutputAzurePostgresId];
        var postgresSecretNameAdmin = outputVariables[TerraformVariables.OutputAzurePostgresSecretNameAdmin];
        var postgresSecretNamePassword = outputVariables[TerraformVariables.OutputAzurePostgresSecretNamePassword];
        var postgresServerName = outputVariables[TerraformVariables.OutputAzurePostgresServerName];

        Assert.That(processedResourceJsonContent, Is.Not.Null);
        Assert.That(processedResourceJsonContent!["postgres_id"], Is.EqualTo(postgresId.Value));
        Assert.That(processedResourceJsonContent!["postgres_dns"], Is.EqualTo(postgresDns.Value));
        Assert.That(processedResourceJsonContent!["postgres_db_name"], Is.EqualTo(postgresDatabaseName.Value));
        Assert.That(processedResourceJsonContent!["postgres_secret_name_admin"],
            Is.EqualTo(postgresSecretNameAdmin.Value));
        Assert.That(processedResourceJsonContent!["postgres_secret_name_password"],
            Is.EqualTo(postgresSecretNamePassword.Value));
        Assert.That(processedResourceJsonContent!["postgres_server_name"], Is.EqualTo(postgresServerName.Value));

        var processedResourceInputJsonContent =
            JsonSerializer.Deserialize<Dictionary<string, string>>(processedResource!.InputJsonContent,
                deserializeOptions);

        Assert.That(processedResourceInputJsonContent, Is.Not.Null);
    }

    [Test]
    [Ignore("Needs to be fixed")]
    public async Task ShouldProcessWorkspaceTemplateOutputVariables()
    {
        var project = new Datahub_Project()
        {
            Project_Acronym_CD = "NEWWORKSPACE",
            Resources = new List<Project_Resources2>()
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var currentPortalUser = new PortalUser
        {
            Email = "tst",
            GraphGuid = Guid.NewGuid().ToString(),
        };

        _context.PortalUsers.Add(currentPortalUser);
        await _context.SaveChangesAsync();

        var terraformOutput = TerraformOutputHelper.GetExpectedTerraformOutput(project);
        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        var outputVariables =
            JsonSerializer.Deserialize<Dictionary<string, TerraformOutputVariable>>(terraformOutput,
                deserializeOptions);
        
        await _terraformOutputHandler.ProcessWorkspaceStatus(outputVariables!);
        
        var updatedProject = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == project.Project_Acronym_CD);
        
        
        Assert.That(updatedProject, Is.Not.Null);
    }
}