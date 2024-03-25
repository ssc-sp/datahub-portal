using BoDi;
using Microsoft.Extensions.Configuration;
using ResourceProvisioner.Application.Config;

namespace ResourceProvisioner.SpecflowTests.Hooks;

[Binding]
public class Hooks
{
    [BeforeScenario("infra-sp")]
    public void BeforeScenarioRequiringQueue(IObjectContainer objectContainer, ScenarioContext scenarioContext)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json", optional: true)
            .Build();

        var resourceProvisionerConfiguration = new ResourceProvisionerConfiguration();
        configuration.Bind(resourceProvisionerConfiguration);

        // register dependencies
        objectContainer.RegisterInstanceAs(resourceProvisionerConfiguration);
    }
}