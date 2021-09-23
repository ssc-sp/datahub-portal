using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NRCan.Datahub.Portal.Services
{
    public class WorkspaceViewModel
    {
        public Group Workspace;
        public IList<Dashboard> Dashboards;
        public IList<Report> Reports;
        public IList<Dataset> Datasets;
        public IList<Dataflow> Dataflows;
    }

    public record PowerBIDatasetElements(Group Group, Dataset Dataset, List<Report> Reports);


    public class PowerBiServiceApi
    {

        private ITokenAcquisition tokenAcquisition { get; }
        private string urlPowerBiServiceApiRoot { get; }

        public const string POWERBI_ROOT_URL = "https://api.powerbi.com/";

        public PowerBiServiceApi(ITokenAcquisition tokenAcquisition)
        {
            //IConfiguration configuration, 
            this.urlPowerBiServiceApiRoot = POWERBI_ROOT_URL;
            this.tokenAcquisition = tokenAcquisition;
        }

        public static readonly string[] RequiredScopes = new string[] {
    "https://analysis.windows.net/powerbi/api/Group.Read.All",
    "https://analysis.windows.net/powerbi/api/Dashboard.Read.All",
    "https://analysis.windows.net/powerbi/api/Report.ReadWrite.All",
    "https://analysis.windows.net/powerbi/api/Dataset.ReadWrite.All",
    "https://analysis.windows.net/powerbi/api/Dataflow.ReadWrite.All",
    "https://analysis.windows.net/powerbi/api/Content.Create",
    "https://analysis.windows.net/powerbi/api/Workspace.ReadWrite.All"
  };


        public static readonly string[] RequiredReadScopes = new string[] {
    "https://analysis.windows.net/powerbi/api/Group.Read.All",
    "https://analysis.windows.net/powerbi/api/Dashboard.Read.All",
    "https://analysis.windows.net/powerbi/api/Report.Read.All",
    "https://analysis.windows.net/powerbi/api/Dataset.Read.All",
    //"https://analysis.windows.net/powerbi/api/Dataflow.Read.All",   
    "https://analysis.windows.net/powerbi/api/Workspace.Read.All"
  };

        public async Task<string> GetAccessTokenAsync()
        {
            return await this.tokenAcquisition.GetAccessTokenForUserAsync(RequiredReadScopes);
        }

        public async Task<PowerBIClient> GetPowerBiClientAsync()
        {
            var tokenCredentials = new TokenCredentials((await GetAccessTokenAsync()), "Bearer");
            return new PowerBIClient(new Uri(urlPowerBiServiceApiRoot), tokenCredentials);
        }

        public async Task<List<PowerBIDatasetElements>> GetAllDatasetsAsync(string appWorkspaceId = "")
        {
            var allDataSets = new List<(Group, Dataset, List<Report>)>();
            using (var client = await GetPowerBiClientAsync())
            {
                // Get a list of workspaces.
                var workspaces = (await client.Groups.GetGroupsAsync()).Value;
                foreach (var workspace in workspaces)
                {
                    var datasets = (await client.Datasets.GetDatasetsInGroupAsync(workspace.Id)).Value;
                    var cReports = (await client.Reports.GetReportsInGroupAsync(workspace.Id)).Value;
                    allDataSets.AddRange(datasets.Select(e => (workspace, e, cReports.Where(r => r.DatasetId == e.Id).ToList())));
                    
                }
                // my workspace
                //var datasetsRef = await client.Datasets.GetDatasetsAsync();
                //var datasets = datasetsRef.Value.ToList();
                //var reportsRef = await client.Reports.GetReportsAsync();
                //var reports = reportsRef.Value.ToList();
                //var groupRequest = new GroupCreationRequest() {  Name = "Test-APICreate"};
                //var newWorkspace = await client.Groups.CreateGroupAsync(groupRequest, true);
            }
            return allDataSets.Select(tp => new PowerBIDatasetElements(tp.Item1,tp.Item2,tp.Item3)).ToList();
        }

        public async Task<string> GetEmbeddedViewModel(string appWorkspaceId = "")
        {
            using PowerBIClient pbiClient = await GetPowerBiClientAsync();
            
            Object viewModel;
            if (string.IsNullOrEmpty(appWorkspaceId))
            {
                viewModel = new
                {
                    currentWorkspace = "My Workspace",
                    workspaces = (await pbiClient.Groups.GetGroupsAsync()).Value,
                    datasets = (await pbiClient.Datasets.GetDatasetsAsync()).Value,
                    reports = (await pbiClient.Reports.GetReportsAsync()).Value,
                };
            }
            else
            {
                Guid workspaceId = new Guid(appWorkspaceId);
                var workspaces = (await pbiClient.Groups.GetGroupsAsync()).Value;
                var currentWorkspace = workspaces.First((workspace) => workspace.Id == workspaceId);
                viewModel = new
                {
                    workspaces = workspaces,
                    currentWorkspace = currentWorkspace.Name,
                    currentWorkspaceIsReadOnly = currentWorkspace.IsReadOnly,
                    datasets = (await pbiClient.Datasets.GetDatasetsInGroupAsync(workspaceId)).Value,
                    reports = (await pbiClient.Reports.GetReportsInGroupAsync(workspaceId)).Value,
                };
            }

            return JsonConvert.SerializeObject(viewModel);
        }

        public async Task<IList<Group>> GetWorkspaces()
        {
            using PowerBIClient pbiClient = await GetPowerBiClientAsync();
            var workspaces = (await pbiClient.Groups.GetGroupsAsync()).Value;
            return workspaces;
        }

        public async Task<WorkspaceViewModel> GetWorkspaceDetails(string workspaceId)
        {

            using PowerBIClient pbiClient = await GetPowerBiClientAsync();

            string filter = $"id eq '{workspaceId}'";

            var workspaceIdGuid = new Guid(workspaceId);

            return new WorkspaceViewModel
            {
                Workspace = (await pbiClient.Groups.GetGroupsAsync(filter)).Value.First(),
                Dashboards = (await pbiClient.Dashboards.GetDashboardsInGroupAsync(workspaceIdGuid)).Value,
                Reports = (await pbiClient.Reports.GetReportsInGroupAsync(workspaceIdGuid)).Value,
                Datasets = (await pbiClient.Datasets.GetDatasetsInGroupAsync(workspaceIdGuid)).Value,
                Dataflows = (await pbiClient.Dataflows.GetDataflowsAsync(workspaceIdGuid)).Value
            };

        }

        public async Task<string> CreateAppWorkspace(string Name)
        {
            using PowerBIClient pbiClient = await GetPowerBiClientAsync();
            // create new app workspace
            GroupCreationRequest request = new GroupCreationRequest(Name);
            Group aws = pbiClient.Groups.CreateGroup(request);
            // return app workspace ID
            return aws.Id.ToString();
        }

        public async Task DeleteAppWorkspace(string WorkspaceId)
        {
            using PowerBIClient pbiClient = await GetPowerBiClientAsync();
            Guid workspaceIdGuid = new Guid(WorkspaceId);
            pbiClient.Groups.DeleteGroup(workspaceIdGuid);
        }

        public async Task PublishPBIX(string appWorkspaceId, string PbixFilePath, string ImportName)
        {
            using PowerBIClient pbiClient = await GetPowerBiClientAsync();
            var stream = new FileStream(PbixFilePath, FileMode.Open, FileAccess.Read);
            var import = pbiClient.Imports.PostImportWithFileInGroup(new Guid(appWorkspaceId), stream, ImportName);
            Console.WriteLine("Publishing process completed");
        }

    }
}
