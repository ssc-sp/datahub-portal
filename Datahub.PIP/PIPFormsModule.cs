using Datahub.Core;
using Datahub.Core.Data;
using Datahub.Portal.Data.PIP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.PIP
{
    public class PIPFormsModule : IDatahubModule
    {
        public void ConfigureDatabases(IServiceCollection serviceProvider, IConfiguration configuration)
        {
            serviceProvider.ConfigureDbContext<PIPDBContext>(configuration, "datahub-mssql-pip", configuration.GetDriver());
        }

        public void InitializeDatabases(DatahubModuleContext ctx, IConfiguration configuration)
        {
            ctx.InitializeDatabase<PIPDBContext>(configuration);
        }
    }
}
