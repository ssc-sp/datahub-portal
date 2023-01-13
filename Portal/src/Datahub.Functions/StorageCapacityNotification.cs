using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions
{
    public class StorageCapacityNotification
    {
        private readonly ILogger _logger;

        public StorageCapacityNotification(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<StorageCapacityNotification>();
        }

        [Function("StorageCapacityNotification")]
        public void Run([QueueTrigger("storage-capacity", Connection = "datahub-storage-queue")] string myQueueItem)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }

        [Function("StorageCapacityNotificationHttp")]
        public async Task<HttpResponseData> RunHttp([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            var myQueueItem = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString($"Payload: {myQueueItem}");

            return response;
        }
    }
}
