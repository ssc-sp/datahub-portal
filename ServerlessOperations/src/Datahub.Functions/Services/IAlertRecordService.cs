using Datahub.Functions.Entities;
using Datahub.Infrastructure.Queues.Messages;

namespace Datahub.Functions.Services
{
    public interface IAlertRecordService
    {
        Task<ReceivedAlert?> GetRecentAlertForBugMessage(BugReportMessage bugReportMessage);
        Task<ReceivedAlert> RecordReceivedAlert(BugReportMessage bugReportMessage, bool sent = true);
    }
}
