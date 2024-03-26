/*
 * PREREQUISITES
 * The Azure function Identity needs to be created as an external user in all databases (except dh-portal-projectdb) and granted  ALTER ROLE [db_owner] ADD MEMBER [dh-portal-func-dev]; 
 * The Azure function Identity needs to be created as an external user in [dh-portal-projectdb] and granted read access to a view that will be created later
    ON PROJECT DB:
    CREATE USER [dh-portal-func-dotnet-dev] FROM EXTERNAL PROVIDER
    ALTER ROLE [db_datareader] ADD MEMBER [dh-portal-func-dotnet-dev]; 
 * The Azure function Identity needs to be have Directory Readers role in azure portal

 
 *  ON EACH INDIVIDUAL DB where users need to be created
    CREATE USER  [dh-portal-func-dotnet-dev] FROM EXTERNAL PROVIDER
    ALTER ROLE [db_owner] ADD MEMBER [dh-portal-func-dotnet-dev]; 

 * 
 * 
*/

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace SyncDbUsers;

public static class SyncDbUsersFunction
{
    [FunctionName("SyncDbUsers")]
    public static async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        try
        {
            //var logger = executionContext.GetLogger("SyncDbUsers");
            //logger.LogInformation("C# HTTP trigger function processed a request.");

            //
            //response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            var content = new StringBuilder();
            var projects = await new ProjectFactory(new EnvConfiguration()).GetUsersFromProjects().ConfigureAwait(false);

            content.AppendLine($"{System.Environment.GetEnvironmentVariable("projectDbConnectionString")}\n");

            if (projects == null)
            {
                content.AppendLine("No projects found");
            }
            else
            {
                foreach (var project in projects)
                {
                    var logs = await new DatabaseUsersCreator(project).Create();
                    foreach (var log in logs)
                    {
                        content.AppendLine($"{log}\n");
                    }
                }
            }

            var token = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/");
            content.AppendLine($"{token}\n");

            return new OkObjectResult(content.ToString());
        }
        catch (Exception ex)
        {
            //var response1 = req.CreateResponse(HttpStatusCode.OK);

            //response1.WriteString($"{System.Environment.GetEnvironmentVariable("projectDbConnectionString")}\n");
            throw;
            //var token = await (new AzureServiceTokenProvider()).GetAccessTokenAsync("https://database.windows.net/");
            //response1.WriteString($"{token}\n");

            // write the error
            //return new ErrorObject (ex.ToString());
            //return response1;
        }
    }
}