namespace Datahub.Functions.Domain.Exceptions
{
    public class BlobDownloadException(string message) : Exception(message);

    public class BlobUploadException(string message) : Exception(message);

    public class CostUpdateException(string message) : Exception(message);

    public class CostRefreshException(string message) : Exception(message);

    public class BudgetUpdateException(string message) : Exception(message);

    public class RolloverException(string message) : Exception(message);

    public class CostQueryException(string message) : Exception(message);

    public class ProjectFilteringException(string message) : Exception(message);

    public class MessageSchedulingException(string message) : Exception(message);
}