using Datahub.Core;
using Datahub.Core.Data;
using Datahub.PIP.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.PIP;

public class PIPFormsModule : IDatahubModule
{
    public void ConfigureDatabases(IServiceCollection serviceProvider, IConfiguration configuration)
    {
        serviceProvider.ConfigureDbContext<PIPDBContext>(configuration, "datahub_mssql_pip", configuration.GetDriver());
    }

    public void InitializeDatabases(DatahubModuleContext ctx, IConfiguration configuration)
    {
        ctx.InitializeDatabase<PIPDBContext>(configuration);
    }
}