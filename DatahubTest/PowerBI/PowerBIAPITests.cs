using Azure.Identity;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Datahub.Tests.PowerBI
{
    public class PowerBIAPITests
    {
        //Resource Uri for Power BI API
        private const string resourceUri = "https://analysis.windows.net/powerbi/api";

        private const string authorityUri = "https://login.windows.net/common/oauth2/authorize";

        private const string graphUri = "https://graph.microsoft.com/.default";
        private const string powerbiUri = "https://analysis.windows.net/powerbi/api/.default";

        public static readonly string[] RequiredScopes = new string[] {
    "https://analysis.windows.net/powerbi/api/Group.Read.All",
    "https://analysis.windows.net/powerbi/api/Dashboard.Read.All",
    "https://analysis.windows.net/powerbi/api/Report.Read.All",
    "https://analysis.windows.net/powerbi/api/Dataset.Read.All",
    "https://analysis.windows.net/powerbi/api/Dataflow.Read.All",
    "https://analysis.windows.net/powerbi/api/Content.Create",
    "https://analysis.windows.net/powerbi/api/Workspace.ReadWrite.All"
  };

        [Fact]
        public async Task GivenCurrentUser_ListWorkspaces()
        {
            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions() { ExcludeInteractiveBrowserCredential = false, ExcludeVisualStudioCodeCredential = true  });
            var token = await credential.GetTokenAsync(
                new Azure.Core.TokenRequestContext(
                    new[] { powerbiUri })
                );

            var serviceCredentials = new TokenCredentials(token.Token, "Bearer");
            var allDataSets = new List<Dataset>();
            using (var client = new PowerBIClient(serviceCredentials))
            {
                //var pipDataSet = await client.Datasets.GetDatasetAsync("aaade8b3-1c90-4b4d-926e-d093bb3e6839");
                // Get a list of workspaces.
                var workspaces = (await client.Groups.GetGroupsAsync()).Value;
                foreach (var workspace in workspaces)
                {
                    
                    allDataSets.AddRange((await client.Datasets.GetDatasetsInGroupAsync(workspace.Id)).Value);
                    var cReports = (await client.Reports.GetReportsInGroupAsync(workspace.Id)).Value;
                }
                // my workspace
                //var datasetsRef = await client.Datasets.GetDatasetsAsync();
                //var datasets = datasetsRef.Value.ToList();
                //var reportsRef = await client.Reports.GetReportsAsync();
                //var reports = reportsRef.Value.ToList();
                //var groupRequest = new GroupCreationRequest() {  Name = "Test-APICreate"};
                //var newWorkspace = await client.Groups.CreateGroupAsync(groupRequest, true);
            }
            Assert.True(allDataSets.Count > 0);      
            //Assert.True(allDataSets.Count(d => d.EndorsementDetails != null) > 0);
        }
    }
}
