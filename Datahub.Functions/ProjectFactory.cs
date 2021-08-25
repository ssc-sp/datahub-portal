using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NRCanDataHub
{
    public class ProjectFactory
    {
     
        private IAppConfig configuration;
        private string projectCode;
        public ProjectFactory(IAppConfig configuration, string projectCode = null)
        {
            this.configuration = configuration;
            this.projectCode = projectCode;
        }
      
        public async Task<IEnumerable<Project>> GetUsersFromProjects(string projectCode = null)
        {
            using (SqlConnection connection = new SqlConnection(this.configuration.GetConnectionString()/*System.Environment.GetEnvironmentVariable("projectDbConnectionString")*/))
            {
                connection.AccessToken = await (new AzureServiceTokenProvider()).GetAccessTokenAsync("https://database.windows.net/");
                connection.Open();

                var cmdSQL = @"WITH 
Usr as (select pu.*, (select top 1 User_name from [Access_Requests] a where a.User_ID = pu.user_id) as User_Name from project_users pu)
,ProjectUsers as 
(Select [Project_Acronym_CD],project_admin, (select STRING_AGG(User_name, ';') from Usr i where i.project_id=p.project_id) as project_non_admin from projects p) 
select Project_Acronym_CD, project_admin, project_non_admin from ProjectUsers where (project_admin is not null or project_non_admin is not null) and Project_Acronym_CD='PIP';
";


                //text = "select 'prj admin ' as project_admin,'accr' as [Project_Acronym_CD] from sys.database_principals";
                using (var command = new SqlCommand(cmdSQL, connection))
                {
                    var rows = await command.ExecuteReaderAsync();
                    var projects = new List<Project>();
                    if (rows.HasRows)
                    {
                        while (rows.Read())
                        {
                            var accronym = rows[0] == DBNull.Value ? "" : rows.GetString(0);
                            var admins = rows[1] == DBNull.Value ? "" : rows.GetString(1);
                            var nonAdmins = rows[2] == DBNull.Value ? "" : rows.GetString(2);
                            var dbConnString = accronym == null ? "" : string.Format(this.configuration.GetConnStringTemplate(), accronym);
                            projects.Add(Project.FromData(accronym, dbConnString, admins, nonAdmins));
                        }
                    }
                    return projects;
                }
            }
        }
    }
}
