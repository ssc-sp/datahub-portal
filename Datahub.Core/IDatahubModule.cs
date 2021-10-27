﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core
{
    public interface IDatahubModule
    {
        void InitializeDatabases(DatahubModuleContext ctx, IConfiguration configuration);
        void ConfigureDatabases(IServiceCollection serviceProvider, IConfiguration configuration);
    }
}
