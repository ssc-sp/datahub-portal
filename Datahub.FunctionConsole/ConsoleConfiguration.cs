using System;
using Microsoft.Extensions.Configuration;
using NRCanDataHub;

namespace SyncDBUsersConsole
{
    public class ConsoleConfiguration : IAppConfig
    {
        private IConfiguration configuration;
        public ConsoleConfiguration()
        {
            this.configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json")
                .Build();
        }

        public string GetConnectionString()
        {
            var section = this.configuration.GetSection("projectDbConnectionString");
            return section.Value;
        }

        public string GetConnStringTemplate()
        {
            var section = this.configuration.GetSection("dbConnStringTemplate");
            return section.Value;
        }

    }
}
