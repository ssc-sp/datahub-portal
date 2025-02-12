namespace Datahub.Application.Services.Toolbox
{
    public interface IToolboxService
    {
        public List<ToolboxTransaction> BeginTransaction();
    }

    public class ToolboxTransaction
    {
        public string Tool { get; set; }
        public ToolboxTransactionType Type { get; set; }
        public dynamic? OriginalData { get; set; }
        public dynamic? UpdatedData { get; set; }
    }

    public enum ToolboxTransactionType
    {
        Add,
        Remove,
        Update
    }
}