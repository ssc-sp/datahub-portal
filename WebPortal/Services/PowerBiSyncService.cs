using Datahub.Core.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerBI.Api.Models;

namespace Datahub.Portal.Services
{
    public class PowerBiSyncService
    {
        private readonly PowerBiServiceApi powerBiServiceApi;
        private readonly IDbContextFactory<DatahubProjectDBContext> dbFactoryProject;

        public PowerBiSyncService(PowerBiServiceApi powerBiServiceApi, IDbContextFactory<DatahubProjectDBContext> DbFactoryProject)
        {
            this.powerBiServiceApi = powerBiServiceApi;
            dbFactoryProject = DbFactoryProject;
        }
        
        public Project_PBI_Workspace? GetWorkspace(Group group)
        {
            using var ctx = dbFactoryProject.CreateDbContext();
            var workspace = ctx.Find<Project_PBI_Workspace>(group.Id);
            if (workspace == null)
            {

            }
        }

        public Project_PBI_Report? GetReport(Report report, Datahub_Project project, Group workspace, DatahubProjectDBContext current_ctx = null)
        {
            using var local_ctx = dbFactoryProject.CreateDbContext();
            var ctx = current_ctx ?? local_ctx;

        }

        public Project_PBI_Report? GetReport(Report report, Datahub_Project project, Project_PBI_Workspace? workspace, DatahubProjectDBContext current_ctx = null)
        {            
            using var local_ctx = dbFactoryProject.CreateDbContext();
            var ctx = current_ctx ?? local_ctx;
            var dbReport = ctx.Find<Project_PBI_Report>(report.Id);
            if (dbReport is null)
            {
                dbReport = new Project_PBI_Report() { Id = report.Id, Workspace = workspace, Project = project, ReportName = report.Name };
                ctx.Add(dbReport);
                if (current_ctx is null) ctx.SaveChanges();
            }
            return dbReport; 
        }
    }
}
