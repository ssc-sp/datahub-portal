using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Shared
{
    public interface IDatahubModule
    {
        void InitializeDatabases(DatahubModuleContext ctx);
        void ConfigureDatabases(IServiceCollection serviceProvider, IConfiguration configuration);
    }
}
