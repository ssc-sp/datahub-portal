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

using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using Azure;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Services.AppAuthentication;

namespace NRCanDataHub
{
    public static class SyncDbUsersFunction
    {
        [Function("SyncDbUsers")]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            try
            {
                //var logger = executionContext.GetLogger("SyncDbUsers");
                //logger.LogInformation("C# HTTP trigger function processed a request.");

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                var projects = await (new ProjectFactory(new EnvConfiguration())).GetUsersFromProjects().ConfigureAwait(false);

                response.WriteString($"{System.Environment.GetEnvironmentVariable("projectDbConnectionString")}\n");

                if (projects == null)
                {
                    response.WriteString("No projects found");
                }
                else
                {
                    foreach (var project in projects)
                    {
                        var logs = await (new DatabaseUsersCreator(project)).Create();
                        foreach (var log in logs)
                        {
                            response.WriteString($"{log}\n");
                        }
                    }
                }

                var token = await (new AzureServiceTokenProvider()).GetAccessTokenAsync("https://database.windows.net/");
                response.WriteString($"{token}\n");

                return response;
            }
            catch (Exception ex)
            {
                var response1 = req.CreateResponse(HttpStatusCode.OK);

                response1.WriteString($"{System.Environment.GetEnvironmentVariable("projectDbConnectionString")}\n");

                var token = await (new AzureServiceTokenProvider()).GetAccessTokenAsync("https://database.windows.net/");
                //response1.WriteString($"{token}\n");

                // write the error
                response1.WriteString(ex.ToString());
                return response1;
            }
        }
    }
}
