using Datahub.Shared.Entities;

namespace Datahub.Application.Services.Toolbox
{
    public interface IToolboxService
    {
        public List<ToolboxTransaction> BeginTransaction();

        public WorkspaceDefinition ApplyTransaction(WorkspaceDefinition workspaceDefinition,
            List<ToolboxTransaction> transactions);
    }

    public class ToolboxTransaction
    {
        public required string Tool { get; set; }
        public ToolboxTransactionType Type { get; set; }
        public dynamic? OriginalData { get; set; }
        public dynamic? UpdatedData { get; set; }

        public override string ToString()
        {
            var baseString = $"{Type.ToString().ToUpper()} {Tool}";
            if (OriginalData != null)
            {
                baseString += $" {OriginalData.GetType().ToString()}";
            }

            if (UpdatedData != null)
            {
                baseString += $" -> {UpdatedData.GetType().ToString()}";
            }

            return baseString;
        }
    }

    public enum ToolboxTransactionType
    {
        Add,
        Remove,
        Update
    }
}