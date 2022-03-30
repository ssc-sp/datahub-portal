using Datahub.Portal.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

namespace Datahub.Portal.Services
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

    public record PowerBiWorkspaceDataset(Guid WorkspaceId, Dataset Dataset);
    public record PowerBiWorkspaceReport(Guid WorkspaceId, Report Report);

    public class PowerBiServiceApi
    {

        private ITokenAcquisition tokenAcquisition { get; }

        private readonly ILogger<PowerBiServiceApi> logger;

        private string urlPowerBiServiceApiRoot { get; }

        public const string POWERBI_ROOT_URL = "https://api.powerbi.com/";

        public PowerBiServiceApi(ITokenAcquisition tokenAcquisition, ILogger<PowerBiServiceApi> logger)
        {
            //IConfiguration configuration, 
            this.urlPowerBiServiceApiRoot = POWERBI_ROOT_URL;
            this.tokenAcquisition = tokenAcquisition;
            this.logger = logger;
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
    //"https://analysis.windows.net/powerbi/api/Group.Read.All",
    "https://analysis.windows.net/powerbi/api/Dashboard.Read.All",
    //"https://analysis.windows.net/powerbi/api/Report.Read.All",
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

        public async Task<Report> GetReport(string report, string workspace)
        {
            using (var client = await GetPowerBiClientAsync())
            {
                return await client.Reports.GetReportInGroupAsync(
                        new Guid(workspace),
                        new Guid(report));
            }
        }

        public async Task<Report> GetReport(string report)
        {
            using (var client = await GetPowerBiClientAsync())
            {
                return await client.Reports.GetReportAsync(new Guid(report));
            }
        }

        public async Task<string> GetReportToken(string report, string workspace)
        {
            var generateTokenRequestParameters =
                new GenerateTokenRequest(accessLevel: "view");
            using (var client = await GetPowerBiClientAsync())
            {
                return (await client.Reports.GenerateTokenInGroupAsync(
                        new Guid(workspace),
                        new Guid(report),
                        generateTokenRequestParameters)).Token;
            }
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
                    try
                    {
                        var datasets = (await client.Datasets.GetDatasetsInGroupAsync(workspace.Id)).Value;
                        var cReports = (await client.Reports.GetReportsInGroupAsync(workspace.Id)).Value;
                        allDataSets.AddRange(datasets.Select(e => (workspace, e, cReports.Where(r => r.DatasetId == e.Id || r.ReportType == "PaginatedReport").ToList())));
                    } catch (Exception ex)
                    {
                        logger.LogWarning(ex, $"Cannot read datasets and reports in workspace {workspace.Id}");
                    }
                    
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

        public async Task<List<PowerBiWorkspaceDataset>> GetWorkspaceDatasetsAsync(Guid? workspaceId = null)
        {
            using var client = await GetPowerBiClientAsync();
            var groups = await client.Groups.GetGroupsAsync();
            var workspaceIds = workspaceId.HasValue ? new List<Guid> { workspaceId.Value } : groups.Value.Select(w => w.Id);

            var result = new List<PowerBiWorkspaceDataset>();
            
            foreach (var id in workspaceIds)
            {
                try
                {
                    var datasets = await client.Datasets.GetDatasetsInGroupAsync(id);
                    result.AddRange(datasets.Value.Select(d => new PowerBiWorkspaceDataset(id, d)));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Cannot get datasets for workspace {id}");
                }
            }

            return result;
        }

        public async Task<List<PowerBiWorkspaceReport>> GetWorkspaceReportsAsync(Guid? workspaceId = null)
        {
            using var client = await GetPowerBiClientAsync();
            var groups = await client.Groups.GetGroupsAsync();
            var workspaceIds = workspaceId.HasValue ? new List<Guid> { workspaceId.Value } : groups.Value.Select(w => w.Id);

            var result = new List<PowerBiWorkspaceReport>();

            foreach(var id in workspaceIds)
            {
                try
                {
                    var reports = await client.Reports.GetReportsInGroupAsync(id);
                    result.AddRange(reports.Value.Select(r => new PowerBiWorkspaceReport(id, r)));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Cannot get reports for workspace {id}");
                }
            }

            return result;
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

        public async Task<List<PowerBiAdminGroupUser>> AssignUsersToWorkspace(Guid workspaceId, List<PowerBiAdminGroupUser> users)
        {
            using var pbiClient = await GetPowerBiClientAsync();

            var errorUsers = await Task.WhenAll(users.Select(async u =>
            {
                var groupUser = new GroupUser(u.UserEmail, PrincipalType.User, u.IsAdmin ? GroupUserAccessRight.Admin : GroupUserAccessRight.Viewer);
                try
                {
                    await pbiClient.Groups.AddGroupUserAsync(workspaceId, groupUser);
                    // if successful, we don't add the user to the error list
                    return null;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Couldn't add user {u.UserEmail} to PowerBI workspace with id {workspaceId}");
                    return u;
                }
            }));
            
            return errorUsers.Where(u => u != null).ToList();
        }

        public async Task TestCreateUser(Guid workspaceId, string userId)
        {
            using var pbiClient = await GetPowerBiClientAsync();
            var newUser = new GroupUser(userId, PrincipalType.User, GroupUserAccessRight.Viewer);

            var groupUsers = await pbiClient.Groups.GetGroupUsersAsync(workspaceId);

            try
            {
                await pbiClient.Groups.AddGroupUserWithHttpMessagesAsync(workspaceId, newUser);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
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
