using Datahub.Shared.Entities;
using Datahub.Shared.Entities.WorkspaceToolConfiguration;

namespace Datahub.Application.Services.Toolbox
{
    public interface IToolboxService
    {
        /// <summary>
        /// Simple method to build us a transaction object to track changes to a workspace definition.
        /// I wrote this here because in the future, a transaction might become a more complicated object.
        /// </summary>
        /// <returns>An empty list of ToolboxTransactions</returns>
        public List<ToolboxTransaction> BeginTransaction();

        /// <summary>
        /// Builds a brand-new workspace definition using a copy of the given workspace definition
        /// and the given transactions. It applies each transaction step by step to the workspace definition.
        /// </summary>
        /// <param name="workspaceDefinition">The original workspace definition to apply the transactions to</param>
        /// <param name="transactions">The list of transactions to apply</param>
        /// <returns>A new workspace definition with the transactions applied</returns>
        public WorkspaceDefinition ApplyTransaction(WorkspaceDefinition workspaceDefinition,
            List<ToolboxTransaction> transactions);
    }

    /// <summary>
    /// A transaction object that represents a unit change to a workspace definition.
    /// The original and updated data are dynamic to allow for any type of data to be used.
    /// They are meant to be used to track changes in associated configuration of a tool.
    /// This allows to easily revert changes to a workspace definition.
    /// </summary>
    public class ToolboxTransaction
    {
        public required string Tool { get; init; }
        public ToolboxTransactionType Type { get; init; }
        public object? OriginalData { get; init; }
        public object? UpdatedData { get; init; }

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

    /// <summary>
    /// Enum to represent the different types of transactions that can be performed on a workspace
    /// definition.
    /// </summary>
    public enum ToolboxTransactionType
    {
        Add,
        Remove,
        Update
    }
}