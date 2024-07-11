using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace Datahub.Functions.UnitTests
{
    [SetUpFixture]
    public class Testing
    {
        internal static IConfiguration _configuration = null!;
        internal static AzureConfig _azureConfig = null!;
        internal static IDbContextFactory<DatahubProjectDBContext> _dbContext = null!;
        
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: true)
                .Build();
            
            _dbContext = Substitute.For<IDbContextFactory<DatahubProjectDBContext>>();
            _azureConfig = new AzureConfig(_configuration);
            _configuration.Bind("AzureAd", _azureConfig);
        }
    }
}