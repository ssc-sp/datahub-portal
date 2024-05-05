using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Infrastructure.Services.Queues
{
    public static class QueueConstants
    {
        public const string QUEUE_EMAILS = "email-notification";
        public const string QUEUE_BUG_REPORT = "bug-report";
        public const string QUEUE_RESOURCE_RUN = "resource-run-request";
        public const string QUEUE_TERRAFORM = "terraform-output";
        public const string QUEUE_STORAGE_CAPACITY = "storage-capacity";
        public const string QUEUE_PROJECT_CAPACITY_UPDATE = "project-capacity-update";
        public const string QUEUE_PROJECT_INACTIVITY = "project-inactivity-notification";
        public const string QUEUE_PROJECT_USAGE = "project-usage-notification";
        public const string QUEUE_PROJECT_USAGE_UPDATE = "project-usage-update";
        public const string QUEUE_USER_INACTIVITY = "user-inactivity-notification";
        public const string QUEUE_WORKSPACE_APP_SERVICE ="workspace-app-service-configuration";
    }
}
