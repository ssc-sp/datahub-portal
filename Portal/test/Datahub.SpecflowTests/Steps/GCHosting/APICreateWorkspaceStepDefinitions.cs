using System;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.GCHosting
{
    [Binding]
    public class APICreateWorkspaceStepDefinitions
    {
        [Given("a request with {string}")]
        public void GivenARequestWithID(string id)
        {
            throw new PendingStepException();
        }

        [Then("the response should have a {int} status code")]
        public void ThenTheResponseShouldHaveAStatusCode(int p0)
        {
            throw new PendingStepException();
        }
    }
}
