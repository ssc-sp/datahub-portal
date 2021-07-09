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

namespace DatahubTest.PowerBI
{
    public class PowerBIAPITests
    {
        //Resource Uri for Power BI API
        private const string resourceUri = "https://analysis.windows.net/powerbi/api";

        private const string authorityUri = "https://login.windows.net/common/oauth2/authorize";

        private const string graphUri = "https://graph.microsoft.com/.default";

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
            var credential = new DefaultAzureCredential(); 
            var token = await credential.GetTokenAsync(
                new Azure.Core.TokenRequestContext(
                    RequiredScopes)
                );

            var serviceCredentials = new TokenCredentials(token.Token, "Bearer");
            using (var client = new PowerBIClient(serviceCredentials))
            {
                // Get a list of workspaces.
                var workspaces = (await client.Groups.GetGroupsAsync()).Value;
                foreach (var workspace in workspaces)
                {
                    var cDataSets = (await client.Datasets.GetDatasetsInGroupAsync(workspace.Id)).Value;
                    var cReports = (await client.Reports.GetReportsInGroupAsync(workspace.Id)).Value;
                }
                // my workspace
                //var datasetsRef = await client.Datasets.GetDatasetsAsync();
                //var datasets = datasetsRef.Value.ToList();
                //var reportsRef = await client.Reports.GetReportsAsync();
                //var reports = reportsRef.Value.ToList();
                var groupRequest = new GroupCreationRequest() {  Name = "Test-APICreate"};
                var newWorkspace = await client.Groups.CreateGroupAsync(groupRequest, true);
            }
        }
    }
}
