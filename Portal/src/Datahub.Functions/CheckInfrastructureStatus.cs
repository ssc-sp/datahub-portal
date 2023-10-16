using System.Net;
using Datahub.Core.Model.Datahub;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions
{
    public class CheckInfrastructureStatus
    {
        private readonly ILogger _logger;
        private readonly DatahubProjectDBContext _projectDbContext;

        public CheckInfrastructureStatus(ILoggerFactory loggerFactory, DatahubProjectDBContext projectDbContext)
        {
            _logger = loggerFactory.CreateLogger<CheckInfrastructureStatus>();
            _projectDbContext = projectDbContext;
        }

        [Function("CheckInfrastructureStatus")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            int component = 0;
            HttpResponseData response;

            switch (component)
            {
                case 0: // Database
                    bool connectable = _projectDbContext.Database.CanConnect();
                    if (!connectable)
                    {
                        response = req.CreateResponse(HttpStatusCode.InternalServerError);
                        response.WriteString("Cannot connect to database.");
                    }

                    var test = _projectDbContext.Projects.First();
                    if (test == null)
                    {
                        response = req.CreateResponse(HttpStatusCode.InternalServerError);
                        response.WriteString("Cannot retrieve from the database.");
                    }
                    else
                    {
                        response = req.CreateResponse(HttpStatusCode.OK);
                        response.WriteString("Successfully connected and checked database.");
                    }
                    break;
                default:
                    response = req.CreateResponse(HttpStatusCode.BadRequest);
                    response.WriteString("An invalid request was made.");
                    break;
                    
            }

            return response;
        }
    }
}
