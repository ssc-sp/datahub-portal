using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.Core;

public interface IDatahubModule
{
    void InitializeDatabases(DatahubModuleContext ctx, IConfiguration configuration);
    void ConfigureDatabases(IServiceCollection serviceProvider, IConfiguration configuration);
}