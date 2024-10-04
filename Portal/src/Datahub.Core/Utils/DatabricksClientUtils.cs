#nullable enable

using System.Text;
using System.Text.Json;
using Datahub.Core.Model.Projects;
using Datahub.Shared.Entities;
using Microsoft.Azure.Databricks.Client;
using Microsoft.Azure.Databricks.Client.Models;

namespace Datahub.Core.Utils;

/// <summary>
/// Provides methods to extract information from a Databricks.
/// </summary>
public class DatabricksClientUtils
{
    private readonly DatabricksClient _client;
    private readonly string _dataBrickUrl;
    private readonly string _authToken;
    public DatabricksClientUtils(string databricksUrl, string token)
    {
        _client = DatabricksClient.CreateClient(databricksUrl, token);
        _authToken = token;
        _dataBrickUrl = databricksUrl;
    }
    public async Task<bool> VerifyACLStatus()
    {
        // Create a new cluster
        var cluster = new ClusterInfo
        {
            ClusterName = "cluster-name",
            //SparkVersionKey = "latest",
            //NodeTypeID = "Standard_D3_v2",
            //NumWorkers = 1
        };

        var clusterId = await _client.Clusters.Create(cluster);

        // Wait for the cluster to be running
        ClusterInfo clusterInfo;
        do
        {
            await Task.Delay(10000); // wait 10 seconds
            clusterInfo = await _client.Clusters.Get(clusterId);
        }
        while (clusterInfo.State != ClusterState.RUNNING);

        // Execute command to check ACL status
        var command = "dbutils.fs.getAclStatus(\"/path/to/file\")";
        var language = "python";
        var commandPayload = new
        {
            language,
            command
        };

        var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, $"https://{_dataBrickUrl}/api/2.0/commands/execute")
        {
            Content = new StringContent(JsonSerializer.Serialize(commandPayload), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Authorization", $"Bearer {_authToken}");

        var response = await httpClient.SendAsync(request);
        var commandResult = await response.Content.ReadAsStringAsync();

        // Parse the result
        var aclStatus = bool.Parse(commandResult);

        // Delete the cluster
        await _client.Clusters.Delete(clusterId);

        return aclStatus;
    }

    public async Task<string> GetClusterStatus(string clusterId)
    {
        if (string.IsNullOrEmpty(clusterId))
        {
            var allClusters = await GetClusters();
            if (allClusters.Count > 0) clusterId = allClusters[0];
        }
        var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://{_dataBrickUrl}/api/2.0/clusters/get?cluster_id={clusterId}");
        request.Headers.Add("Authorization", $"Bearer {_authToken}");

        var response = await httpClient.SendAsync(request);
        var clusterInfo = await response.Content.ReadAsStringAsync();

        // Parse the cluster state from the response
        var jsonDocument = JsonDocument.Parse(clusterInfo);
        var clusterState = jsonDocument.RootElement.GetProperty("state").GetString() ?? string.Empty;

        return clusterState;
    }

    public async Task<List<string>> GetClusters()
    {
        var reply = new List<string>();
        var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://{_dataBrickUrl}/api/2.0/clusters/list");
        request.Headers.Add("Authorization", $"Bearer {_authToken}");

        try
        {
            var response = await httpClient.SendAsync(request);
            var clusters = await response.Content.ReadAsStringAsync();

            var jsonDocument = JsonDocument.Parse(clusters);
            var clusterArray = jsonDocument.RootElement.GetProperty("clusters").EnumerateArray();

            foreach (var cluster in clusterArray)
            {
                var clusterName = cluster.GetProperty("cluster_name").GetString();
                if (!string.IsNullOrEmpty(clusterName))
                {
                    reply.Add(clusterName);
                }
            }

            return reply;
        }
        catch (Exception)
        {
            // If the request failed, the Databricks instance might not be running
            throw;
        }
    }

    public async Task<bool> IsDatabricksInstanceRunning()
    {
        try
        {
            var allClusters = await GetClusters();
            return true;
        }
        catch (Exception)
        {
            // If the request failed, the Databricks instance might not be running
            return false;
        }
    }
}