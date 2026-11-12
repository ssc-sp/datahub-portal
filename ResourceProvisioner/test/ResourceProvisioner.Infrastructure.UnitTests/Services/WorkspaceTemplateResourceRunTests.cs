using Datahub.Shared;
using Datahub.Shared.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Moq;
using NUnit.Framework;
using ResourceProvisioner.Application.Config;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Infrastructure.Services;

namespace ResourceProvisioner.Infrastructure.UnitTests.Services;

/// <summary>
/// Verifies that the base and enhanced (Protected B) workspace templates are both treated as the workspace template.
/// Git operations are stubbed out, so these tests only exercise the resource run decisions.
/// </summary>
public class WorkspaceTemplateResourceRunTests
{
    private const string Acronym = "TST";
    private Mock<ITerraformService> _terraformService = null!;
    private Mock<RepositoryService> _repositoryService = null!;

    [SetUp]
    public void SetUp()
    {
        _terraformService = new Mock<ITerraformService>();
        _repositoryService = new Mock<RepositoryService>(
            Mock.Of<IHttpClientFactory>(),
            Mock.Of<ILogger<RepositoryService>>(),
            Options.Create(new ResourceProvisionerConfiguration()),
            _terraformService.Object,
            Mock.Of<IFeatureManager>(),
            new ConfigurationBuilder().Build())
        {
            CallBase = true
        };
        _repositoryService
            .Setup(x => x.CommitTerraformTemplate(It.IsAny<TerraformTemplate>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    private static WorkspaceDefinition CreateDefinition(params TerraformTemplate[] templates) => new()
    {
        Templates = templates.ToList(),
        Workspace = new TerraformWorkspace { Acronym = Acronym },
        AppData = new WorkspaceAppData(),
        RequestingUserEmail = "unit-test@example.com",
        ResourceGroupName = "rg-test"
    };

    [TestCase(TerraformTemplate.NewProjectTemplate, TerraformTemplate.AzureDatabricks)]
    [TestCase(TerraformTemplate.NewProjectTemplateEnhanced, TerraformTemplate.AzureDatabricksEnhanced)]
    public async Task ShouldRunWorkspaceTemplateFirstAndWriteBackendConfig(string workspaceTemplate, string toolTemplate)
    {
        var copiedTemplates = new List<string>();
        _terraformService
            .Setup(x => x.CopyTemplateAsync(It.IsAny<string>(), It.IsAny<WorkspaceDefinition>()))
            .Callback<string, WorkspaceDefinition>((name, _) => copiedTemplates.Add(name))
            .Returns(Task.CompletedTask);

        var definition = CreateDefinition(
            new TerraformTemplate(toolTemplate, TerraformStatus.CreateRequested),
            new TerraformTemplate(workspaceTemplate, TerraformStatus.CreateRequested));

        await _repositoryService.Object.ExecuteResourceRuns(definition, "unit-test@example.com");

        Assert.That(copiedTemplates, Is.EqualTo(new[] { workspaceTemplate, toolTemplate }));
        _terraformService.Verify(x => x.ExtractBackendConfig(Acronym), Times.Once);
    }

    [TestCase(TerraformTemplate.NewProjectTemplate)]
    [TestCase(TerraformTemplate.NewProjectTemplateEnhanced)]
    public async Task ShouldDeleteWorkspaceWhenWorkspaceTemplateDeleteRequested(string workspaceTemplate)
    {
        var definition = CreateDefinition(new TerraformTemplate(workspaceTemplate, TerraformStatus.DeleteRequested));

        await _repositoryService.Object.ExecuteResourceRuns(definition, "unit-test@example.com");

        _terraformService.Verify(x => x.DeleteWorkspaceAsync(definition.Workspace, "rg-test"), Times.Once);
        _terraformService.Verify(x => x.DeleteTemplateAsync(It.IsAny<string>(), It.IsAny<TerraformWorkspace>()), Times.Never);
    }
}
