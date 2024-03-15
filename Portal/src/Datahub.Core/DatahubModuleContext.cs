using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Datahub.Core.Data;
using System;
using Microsoft.Extensions.Configuration;

namespace Datahub.Core;

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
        var deleteInOffline = configuration.GetSection("InitialSetup")?.GetValue<bool>("EnsureDeleteinOffline", false) ?? false;
        var resetDb = configuration.GetSection("InitialSetup")?.GetValue<bool>("ResetDB", false) ?? false;
        EFTools.InitializeDatabase<T>(logger, configuration, serviceProvider, resetDb, migrate, deleteInOffline);
    }
}