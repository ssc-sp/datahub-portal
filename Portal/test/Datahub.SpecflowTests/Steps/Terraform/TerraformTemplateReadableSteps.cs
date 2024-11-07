using Datahub.Shared.Entities;
using FluentAssertions;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.Terraform;

[Binding]
public class TerraformTemplateReadableSteps(ScenarioContext scenarioContext)
{
    [Given(@"I have a Terraform template called ""(.*)""")]
    public void GivenIHaveATerraformTemplateCalled(string template)
    {
        var terraformTemplate = TerraformTemplate.GetTerraformServiceType(template);
        scenarioContext["terraformTemplateName"] = terraformTemplate;
    }

    [When(@"I convert the Terraform template to a readable string in ""(.*)""")]
    public void WhenIConvertTheTerraformTemplateToAReadableStringIn(string language)
    {
        var terraformTemplateName = scenarioContext.Get<string>("terraformTemplateName");
        var readableName = TerraformTemplate.ConvertTemplateNameToReadableName(terraformTemplateName, language == "fr");
        scenarioContext["readableName"] = readableName;
    }

    [Then(@"the readable string should be ""(.*)""")]
    public void ThenTheReadableStringShouldBe(string readable)
    {
        var readableName = scenarioContext["readableName"] as string;
        readableName.Should().Match(readable);
    }
}