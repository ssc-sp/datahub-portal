using Datahub.Core;
using Datahub.Core.Data;
using Datahub.Portal.Data.Finance;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.Finance
{
    public class FinanceModule : IDatahubModule
    {
        public void ConfigureDatabases(IServiceCollection serviceProvider, IConfiguration configuration)
        {
            serviceProvider.ConfigureDbContext<FinanceDBContext>(configuration, "datahub-mssql-finance", configuration.GetDriver());
        }

        public void InitializeDatabases(DatahubModuleContext ctx, IConfiguration configuration)
        {
            ctx.InitializeDatabase<FinanceDBContext>(configuration);
        }
    }
}
