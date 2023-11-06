using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Datahub.Core;
using Datahub.Core.Data;
using Datahub.LanguageTraining.Data;

namespace Datahub.LanguageTraining;

public class LanguageTrainingModule : IDatahubModule
{
    public void ConfigureDatabases(IServiceCollection serviceProvider, IConfiguration configuration)
    {
        serviceProvider.ConfigureDbContext<LanguageTrainingDBContext>(configuration, "datahub_mssql_languagetraining", configuration.GetDriver());
    }

    public void InitializeDatabases(DatahubModuleContext ctx, IConfiguration configuration)
    {
        ctx.InitializeDatabase<LanguageTrainingDBContext>(configuration);
    }
}