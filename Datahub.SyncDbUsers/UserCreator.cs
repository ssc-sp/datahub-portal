using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;

namespace NRCanDataHub
{
    /// <summary>
    /// Connects to a database by dynamically building a connection string
    /// and iterates through a list of users, creating the corresponding AD account
    /// </summary>
    public class DatabaseUsersCreator
    {
        private readonly Project project;
        public DatabaseUsersCreator(Project project)
        {
            this.project = project;
        }

        public async Task<IEnumerable<string>> Create()
        {
            var logging = new List<string>();
            logging.Add($"--- Project {this.project.Name} ---------");

            using (var connection = new SqlConnection(project.DBConnString))
            {
                connection.AccessToken = await (new AzureServiceTokenProvider()).GetAccessTokenAsync("https://database.windows.net/");

                logging.Add($"--- Connecting to {connection.Database} ---------");

                try
                {
                    connection.Open();
                    foreach (var user in this.project.Users)
                    {
                        ///this code is for testing only, it should actually create users in the db and return a bool if succeeds
                        var text = BuildCreateUserCmd(user);
                        using (var command = new SqlCommand(text, connection))
                        {
                            try
                            {
                                await command.ExecuteNonQueryAsync();
                                logging.Add($"Created username {user.Username} for {this.project.Name} project");
                            }
                            catch (Exception ex)
                            {
                                logging.Add($"Failed to create the user {user.Username}. Exception: {ex.Message}");

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logging.Add($"Failed to connect to {connection.Database}. Exception: {ex.Message}");
                }
            }

            return logging;
        }

        private string BuildCreateUserCmd(User u)
        {
            var cmdAdminSpecific = u.IsAdmin ? $"ALTER ROLE[db_datawriter] ADD MEMBER[{u.Username}]; ALTER ROLE[db_ddladmin] ADD MEMBER[{u.Username}]; ALTER ROLE[db_securityadmin] ADD MEMBER[{u.Username}]" : "";
            return
                @$"if not exists (select * FROM sys.database_principals where name='{u.Username}') 
                begin
                CREATE USER [{u.Username}] FROM EXTERNAL PROVIDER
                end;
                if exists (select * FROM sys.database_principals where name='{u.Username}') 
                begin
                ALTER ROLE[db_datareader] ADD MEMBER[{u.Username}];
                {cmdAdminSpecific}
                end";
        }


    }
}
