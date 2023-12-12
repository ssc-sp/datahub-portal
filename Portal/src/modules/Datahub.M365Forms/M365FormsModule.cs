using Datahub.Core;
using Datahub.Core.Data;
using Datahub.M365Forms.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.M365Forms;

public class M365FormsModule : IDatahubModule
{
    public void ConfigureDatabases(IServiceCollection serviceProvider, IConfiguration configuration)
    {
        serviceProvider.ConfigureDbContext<M365FormsDBContext>(configuration, "datahub_mssql_m365forms", configuration.GetDriver());
    }

    public void InitializeDatabases(DatahubModuleContext ctx, IConfiguration configuration)
    {
        ctx.InitializeDatabase<M365FormsDBContext>(configuration); 
    }
}