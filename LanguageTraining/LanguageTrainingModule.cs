using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Datahub.Portal.Data.LanguageTraining;
using Datahub.Core;
using Datahub.Core.Data;
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

        public void InitializeDatabases(DatahubModuleContext ctx, IConfiguration configuration)
        {
            ctx.InitializeDatabase<LanguageTrainingDBContext>(configuration);
        }
    }
}
