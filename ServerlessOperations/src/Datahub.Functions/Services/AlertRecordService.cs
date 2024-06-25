using Azure;
using Azure.Data.Tables;
using Datahub.Functions.Entities;
using Datahub.Functions.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Functions.Services
{
    public class AlertRecordService(AzureConfig config) : IAlertRecordService
    {
        private readonly AzureConfig _config = config;

        private const string RECEIVED_ALERTS_TABLE_NAME = "ReceivedAlerts";
        private const string BUG_REPORT_MESSAGES_TABLE_NAME = "BugReportMessages";

        private async Task<TableClient> CreateTableClient(string tableName)
        {
            var connString = _config.StorageQueueConnection;
            var client = new TableClient(connString, tableName);
            await client.CreateIfNotExistsAsync();
            return client;
        }

        public async Task<ReceivedAlert?> GetRecentAlertForBugMessage(BugReportMessage bugReportMessage)
        {
            var client = await CreateTableClient(RECEIVED_ALERTS_TABLE_NAME);
            var reportIdentifier = bugReportMessage.GenerateInfrastructureReportIdentifier();
            
            //TODO configure debounce time
            var earliestTime = DateTimeOffset.Now.AddDays(-1);

            var alertsSinceTime = client.QueryAsync<ReceivedAlert>(r => r.ReportIdentifier ==  reportIdentifier && r.Timestamp >= earliestTime);
            return await alertsSinceTime.OrderByDescending(a => a.Timestamp).FirstOrDefaultAsync();
        }

        public async Task<ReceivedAlert> RecordReceivedAlert(BugReportMessage bugReportMessage)
        {
            var alertClient = await CreateTableClient(RECEIVED_ALERTS_TABLE_NAME);
            var bugReportClient = await CreateTableClient(BUG_REPORT_MESSAGES_TABLE_NAME);

            var savedBugReport = bugReportMessage.CreateSavedBugMessage();
            var bugResponse = await bugReportClient.AddEntityAsync(savedBugReport);
            //TODO check response

            var savedAlert = savedBugReport.GenerateAlertRecord();
            var alertResponse = await alertClient.AddEntityAsync(savedAlert);
            //TODO check response

            return savedAlert;
        }
    }
}
