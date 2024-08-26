namespace Datahub.Functions.Domain.Exceptions
{
   public class BlobDownloadException(string message) : Exception(message);
   public class BlobUploadException(string message) : Exception(message);
   public class WorkspaceCostUpdateException(string message) : Exception(message);
   public class WorkspaceBudgetUpdateException(string message) : Exception(message);
   public class WorkspaceRolloverException(string message) : Exception(message);
   public class CostQueryException(string message) : Exception(message);
}