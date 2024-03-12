using Datahub.Application.Services.Azure;
using Datahub.Application.Services.Budget;
using Datahub.Infrastructure.Services.Azure;
using Microsoft.Extensions.Logging;
using static Datahub.Infrastructure.UnitTests.Testing;
using NSubstitute;

namespace Datahub.Infrastructure.UnitTests.Services
{
    [TestFixture]
    public class WorkspaceBudgetManagementServiceTests
    {
        private IAzureResourceManagerClientProvider _armClientProvider;
        private IWorkspaceBudgetManagementService _sut;
        private readonly ILoggerFactory _loggerFactory;
        
        [SetUp]
        public void SetUp()
        {
            _armClientProvider = new AzureResourceManagerClientProvider(_loggerFactory.CreateLogger<AzureResourceManagerClientProvider>());
            _armClientProvider.Initialize();
        }
    }
}