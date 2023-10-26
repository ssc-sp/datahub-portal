using Datahub.Core;
using Datahub.Core.Data;
using Datahub.Finance.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.Finance;

public class FinanceModule : IDatahubModule
{
    public void ConfigureDatabases(IServiceCollection serviceProvider, IConfiguration configuration)
    {
        //trigger a build
        serviceProvider.ConfigureDbContext<FinanceDBContext>(configuration, "datahub_mssql_finance", configuration.GetDriver());
    }

    public void InitializeDatabases(DatahubModuleContext ctx, IConfiguration configuration)
    {
        ctx.InitializeDatabase<FinanceDBContext>(configuration);
    }
}