using Microsoft.Azure.Services.AppAuthentication;
using NRCanDataHub;
using System;
using System.Threading.Tasks;


namespace SyncDBUsersConsole
{



    class Program
    {
        static async Task Main(string[] args)
        {
            var projectCode = args.Length > 0 ? args[0] : null;
           
            try
            {
                var projects = await (new ProjectFactory(new ConsoleConfiguration(), projectCode)).GetUsersFromProjects().ConfigureAwait(false);
                if (projects == null)
                {
                    Console.WriteLine("No projects found");
                }
                else
                {
                    foreach (var project in projects)
                    {
                        var logs = await (new DatabaseUsersCreator(project)).Create();
                        Console.WriteLine(string.Join("\n", logs));
                    }
                }
                var token = await (new AzureServiceTokenProvider()).GetAccessTokenAsync("https://database.windows.net/");
                Console.WriteLine($"{token}\n");


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
