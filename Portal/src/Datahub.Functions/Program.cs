using Datahub.Core.Model.Datahub;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
    })
    .ConfigureServices(services =>
    {
        //
        var config = new ConfigurationBuilder()
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .Build();
        
        var connectionString = config["datahub-mssql-project"];
        if (connectionString is not null)
        {
            services.AddPooledDbContextFactory<DatahubProjectDBContext>(options =>
                options.UseSqlServer(connectionString));
            services.AddDbContextPool<DatahubProjectDBContext>(options => options.UseSqlServer(connectionString));
        }

    })
    .Build();

host.Run();
