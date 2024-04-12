using Datahub.Application.Services.Reports;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Datahub.Infrastructure.Services.Reports
{
    public class WorkspaceReportsManagementService : IWorkspaceReportsManagementService
    {
        private readonly DbContextFactory<DatahubProjectDBContext> _dbContextFactory;
        private readonly IMediator _mediator;

        public WorkspaceReportsManagementService(DbContextFactory<DatahubProjectDBContext> dbContextFactory,
            IMediator mediator)
        {
            _dbContextFactory = dbContextFactory;
            _mediator = mediator;
        }

        public async Task<bool> CreateReportAsync(string workspaceAcronym, WorkspaceReport report)
        {
            using var ctx = _dbContextFactory.CreateDbContext();
            var project = ctx.Projects
                .Include(p => p.Reports)
                .FirstOrDefault(p => p.Project_Acronym_CD == workspaceAcronym);

            if (project is null)
            {
                throw new ArgumentException($"Workspace with acronym {workspaceAcronym} not found");
            }

            var projectReport = RecordToModel(report);
            project.Reports.Add(projectReport);
            ctx.SaveChanges();

            return true;
        }

        public async Task<List<WorkspaceReport>> GetReportsAsync(string workspaceAcronym)
        {
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects
                .Include(p => p.Reports)
                .FirstOrDefaultAsync(p => p.Project_Acronym_CD == workspaceAcronym);

            if (project is null)
            {
                throw new ArgumentException($"Workspace with acronym {workspaceAcronym} not found");
            }

            var reports = project.Reports;

            if (reports is not null)
            {
               var workspaceReports = ModelsToRecords(reports);
               return workspaceReports;
            }

            return new List<WorkspaceReport>();
        }

        internal List<WorkspaceReport> ModelsToRecords(List<ProjectReports> reports)
        {
            return reports.Select(r => new WorkspaceReport(r.GeneratedDate, r.UpdatedDate, r.GeneratedBy,
                (r.CoverageStartDate, r.CoverageEndDate), (WorkspaceReportType)r.ReportType,
                (WorkspaceReportStatus)r.ReportStatus, r.ReportName, r.ReportUrl)).ToList();
        }
        
        internal ProjectReports RecordToModel(WorkspaceReport report)
        {
            return new ProjectReports
            {
                GeneratedBy = report.GeneratedBy,
                CoverageStartDate = report.CoverageDates.Item1,
                CoverageEndDate = report.CoverageDates.Item2,
                ReportType = (int)report.ReportType,
                ReportStatus = (int)report.ReportStatus,
                ReportName = report.ReportName,
                ReportUrl = report.ReportUrl
            };
        }
        
        
    }
}