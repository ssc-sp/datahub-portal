using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;

namespace Datahub.Application.Services.Projects
{
    public interface IProjectInactivityService
    {
        public Task<int> UpdateDaysSinceLastLogin(int projectId, CancellationToken ct);
        public Task<int> GetDaysSinceLastLogin(int projectId, CancellationToken ct);
        public Task<bool> GetProjectWhitelisted(int projectId, CancellationToken ct);
        public Task SetProjectWhitelisted(int projectId, bool whitelisted, CancellationToken ct);
        public Task<DateTime?> GetProjectRetirementDate(int projectId, CancellationToken ct);
        public Task SetProjectRetirementDate(int projectId, DateTime retirementDate, CancellationToken ct);
        public Task SetProjectThresholdNotified(int projectId, int thresholdNotified, CancellationToken ct);
        public Task SetProjectDateLastNotified(int projectId, DateTime dateLastNotified, CancellationToken ct);
    }
}