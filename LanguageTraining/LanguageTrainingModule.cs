using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NRCan.Datahub.Portal.Data.LanguageTraining;
using NRCan.Datahub.Shared;
using NRCan.Datahub.Shared.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.LanguageTraining
{
    public class LanguageTrainingModule : IDatahubModule
    {
        public void ConfigureDatabases(IServiceCollection serviceProvider, IConfiguration configuration)
        {
             serviceProvider.ConfigureDbContext<LanguageTrainingDBContext>(configuration, "datahub-mssql-languagetraining", configuration.GetDriver());
        }

        public void InitializeDatabases(DatahubModuleContext ctx)
        {
            ctx.InitializeDatabase<LanguageTrainingDBContext>();
        }
    }
}
