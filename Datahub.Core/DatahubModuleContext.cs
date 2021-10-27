using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Datahub.Core.Data;
using Datahub.Core.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Datahub.Core
{
    public class DatahubModuleContext
    {

        private IServiceProvider serviceProvider;

        private bool offline;

        private ILogger<DatahubModuleContext> logger;

        public DatahubModuleContext(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            var environment = serviceProvider.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
            this.offline = environment.IsEnvironment("Offline"); ;
            logger = serviceProvider.GetService(typeof(ILogger<DatahubModuleContext>)) as ILogger<DatahubModuleContext>;
        }

        public void InitializeDatabase<T>(IConfiguration configuration, bool migrate = true, bool ensureDeleteinOffline = true) where T : DbContext
        {
            EFTools.InitializeDatabase<T>(logger, configuration, serviceProvider, offline, migrate, ensureDeleteinOffline);
        }


    }
}
