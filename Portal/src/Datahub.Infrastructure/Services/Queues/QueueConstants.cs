namespace Datahub.Infrastructure.Services.Queues
{
    public static class QueueConstants
    {
        public const string QUEUE_PONG = "pong-queue";
        public const string QUEUE_USER_RUN_REQUEST = "user-run-request";
        public const string QUEUE_RESOURCE_RUN_REQUEST = "resource-run-request";
        public const string QUEUE_DELETE_RUN_REQUEST = "delete-run-request";
        public const string QUEUE_USER_INACTIVITY_NOTIFICATION = "user-inactivity-notification";
        public const string QUEUE_PROJECT_INACTIVITY_NOTIFICATION = "project-inactivity-notification";
        public const string QUEUE_PROJECT_CAPACITY = "project-capacity-update";
        public const string QUEUE_PROJECT_USAGE_NOTIFICATION = "project-usage-notification";
        public const string QUEUE_PROJECT_USAGE_UPDATE = "project-usage-update";
        public const string QUEUE_STORAGE_CAPACITY = "storage-capacity";
        public const string QUEUE_TERRAFORM_RUN = "terraform-output";
        public const string QUEUE_EMAILS = "email-notification";
    }
}
