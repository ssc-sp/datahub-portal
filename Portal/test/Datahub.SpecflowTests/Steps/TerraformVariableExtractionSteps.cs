using Datahub.Core.Model.Projects;
using Datahub.Core.Utils;
using FluentAssertions;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps;

[Binding]
public sealed class TerraformVariableExtractionSteps
{
    // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

    private readonly ScenarioContext _scenarioContext;

    public TerraformVariableExtractionSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given(@"a datahub project with the following resources")]
    public void GivenADatahubProjectWithTheFollowingResources(Table table)
    {
        var project = new Datahub_Project();
        
        var projectResources = table.CreateSet<Project_Resources2>()
            .ToList();
        
        project.Resources = projectResources;
        _scenarioContext["project"] = project;
    }

    [When(@"I call the method to extract the databricks workspace id")]
    public void WhenICallTheMethodToExtractTheDatabricksWorkspaceId()
    {
        var project = _scenarioContext["project"] as Datahub_Project;
        var databricksWorkspaceId = TerraformVariableExtraction.ExtractDatabricksWorkspaceId(project);
        _scenarioContext["databricksWorkspaceId"] = databricksWorkspaceId;
    }

    [Then(@"I should get the following value")]
    public void ThenIShouldGetTheFollowingValue(Table table)
    {
        var expectedValue = table.Rows[0]["Value"];
        var name = table.Rows[0]["Name"];
        
        var actualValue = _scenarioContext[name] as string;
        actualValue.Should().Be(expectedValue);
    }

    [When(@"I call the method to extract the databricks workspace url")]
    public void WhenICallTheMethodToExtractTheDatabricksWorkspaceUrl()
    {
        var project = _scenarioContext["project"] as Datahub_Project;
        var databricksUrl = TerraformVariableExtraction.ExtractDatabricksUrl(project);
        _scenarioContext["databricksUrl"] = databricksUrl;
    }

    [When(@"I call the method to extract the postgres host")]
    public void WhenICallTheMethodToExtractThePostgresHost()
    {
        var project = _scenarioContext["project"] as Datahub_Project;
        var postgresHost = TerraformVariableExtraction.ExtractAzurePostgresHost(project);
        _scenarioContext["postgresHost"] = postgresHost;
    }

    [When(@"I call the method to extract the postgres database name")]
    public void WhenICallTheMethodToExtractThePostgresDatabaseName()
    {
        var project = _scenarioContext["project"] as Datahub_Project;
        var postgresDatabaseName = TerraformVariableExtraction.ExtractAzurePostgresDatabaseName(project);
        _scenarioContext["postgresDatabaseName"] = postgresDatabaseName;
    }

    [When(@"I call the method to extract the postgres username secret name")]
    public void WhenICallTheMethodToExtractThePostgresUsernameSecretName()
    {
        var project = _scenarioContext["project"] as Datahub_Project;
        var postgresUsernameSecretName = TerraformVariableExtraction.ExtractAzurePostgresUsernameSecretName(project);
        _scenarioContext["postgresUsernameSecretName"] = postgresUsernameSecretName;
    }

    [When(@"I call the method to extract the postgres password secret name")]
    public void WhenICallTheMethodToExtractThePostgresPasswordSecretName()
    {
        var project = _scenarioContext["project"] as Datahub_Project;
        var postgresPasswordSecretName = TerraformVariableExtraction.ExtractAzurePostgresPasswordSecretName(project);
        _scenarioContext["postgresPasswordSecretName"] = postgresPasswordSecretName;
    }

    [Given(@"the first number is (.*)")]
    public void GivenTheFirstNumberIs(int p0)
    {
        ScenarioContext.StepIsPending();
    }
}