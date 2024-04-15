using Datahub.Shared.Entities;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;

namespace ResourceProvisioner.SpecflowTests.Steps;

[Binding]
public sealed class ResourceRunRequestSteps(ScenarioContext scenarioContext)
{
    [GivenAttribute(@"the user has a workspace definition")]
    public void GivenTheUserHasAWorkspaceDefinition()
    {
        var createResourceRunCommand = new CreateResourceRunCommand()
        {
            Templates = [new TerraformTemplate() { Name = "test" }, new TerraformTemplate() { Name = "test2" }],
            Workspace = new TerraformWorkspace() { Acronym = "test" },
            AppData = new WorkspaceAppData()
            {
                DatabricksHostUrl = "test",
                AppServiceConfiguration = new AppServiceConfiguration()
                {
                    Framework = "test"
                }
            }
        };

        scenarioContext["createResourceRunCommand"] = createResourceRunCommand;
    }
}