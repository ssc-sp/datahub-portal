using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.WebApp;
using Datahub.Core.Model.Context;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Helpers;
using Datahub.Infrastructure.Services.WebApp;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Functions.UnitTests
{
    [TestFixture]
    public class HealthCheckFunctionTests
    {
        private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
        private readonly IConfiguration _config = Substitute.For<IConfiguration>();
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory = Substitute.For<IDbContextFactory<DatahubProjectDBContext>>();
        private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();

        private CheckInfrastructureStatus _checkInfrastructureStatusFunction;

        [SetUp]
        public void Setup()
        {
            var datahubConfig = new DatahubPortalConfiguration();
            _config.Bind(datahubConfig);

            var projectStorageConfigurationService = new ProjectStorageConfigurationService(datahubConfig);

            var sendProvider = Substitute.For<ISendEndpointProvider>();
            var kvUserService = Substitute.For<IKeyVaultUserService>();
            var webAppService = new WorkspaceWebAppManagementService(datahubConfig, _dbContextFactory, sendProvider, kvUserService);

            var resourceMessagingService = new ResourceMessagingService(_dbContextFactory, sendProvider);

            var healthCheckHelper = new HealthCheckHelper(_dbContextFactory, projectStorageConfigurationService, webAppService, 
                _config, _httpClientFactory, _loggerFactory, sendProvider, resourceMessagingService, datahubConfig);

            _checkInfrastructureStatusFunction = new CheckInfrastructureStatus(_loggerFactory, healthCheckHelper);
        }

        [Test]
        public async Task TestCoreAzureSQLDatabaseHealthCheck()
        {
            var healthCheckRequest = new InfrastructureHealthCheckMessage(Core.Model.Health.InfrastructureHealthResourceType.AzureSqlDatabase, 
                InfrastructureHealthCheckConstants.CoreRequestGroup, InfrastructureHealthCheckConstants.CoreRequestGroup);
            var result = await _checkInfrastructureStatusFunction.ProcessRequest(healthCheckRequest);

            result.Should().BeAssignableTo<OkObjectResult>();
        }

        [OneTimeTearDown]
        public void TearDown() 
        { 
            _loggerFactory?.Dispose();
        }
    }
}
