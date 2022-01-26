using Datahub.Core;
using Datahub.Core.Data;
using Datahub.Portal.Data.M365Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.M365Forms
{
    public class M365FormsModule : IDatahubModule
    {
        public void ConfigureDatabases(IServiceCollection serviceProvider, IConfiguration configuration)
        {
            serviceProvider.ConfigureDbContext<M365FormsDBContext>(configuration, "datahub-mssql-m365forms", configuration.GetDriver());
        }

        public void InitializeDatabases(DatahubModuleContext ctx, IConfiguration configuration)
        {
            ctx.InitializeDatabase<M365FormsDBContext>(configuration); 
        }
    }
}
