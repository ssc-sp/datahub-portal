using Datahub.Functions.Entities;
using Datahub.Infrastructure.Queues.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Functions.Services
{
    public interface IAlertRecordService
    {
        Task<ReceivedAlert?> GetRecentAlertForBugMessage(BugReportMessage bugReportMessage);
        Task<ReceivedAlert> RecordReceivedAlert(BugReportMessage bugReportMessage, bool sent = true);
    }
}
