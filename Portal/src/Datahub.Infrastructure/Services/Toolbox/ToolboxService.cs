using System.Reflection;
using System.Text.Json;
using Datahub.Application.Services.Toolbox;
using Datahub.Shared;
using Datahub.Shared.Entities;
using Datahub.Shared.Entities.WorkspaceToolConfiguration;

namespace Datahub.Infrastructure.Services.Toolbox
{
    public class ToolboxService : IToolboxService
    {
        /// <inheritdoc/>
        public List<ToolboxTransaction> BeginTransaction()
        {
            return new List<ToolboxTransaction>();
        }

        /// <inheritdoc/>
        public WorkspaceDefinition ApplyTransaction(WorkspaceDefinition workspaceDefinition,
            List<ToolboxTransaction> transactions)
        {
            WorkspaceDefinition newWorkspaceDefinition;
            var wdString = JsonSerializer.Serialize(workspaceDefinition);
            newWorkspaceDefinition = JsonSerializer.Deserialize<WorkspaceDefinition>(wdString)!;
            var requestedDate = DateTime.UtcNow;
            foreach (var transaction in transactions)
            {
                switch (transaction.Type)
                {
                    case ToolboxTransactionType.Add:
                        newWorkspaceDefinition.Templates.Add(new TerraformTemplate(transaction.Tool,
                            TerraformStatus.CreateRequested, requestedDate));
                        ApplyConfigurations(newWorkspaceDefinition, transaction);
                        break;
                    case ToolboxTransactionType.Remove:
                        newWorkspaceDefinition.Templates.RemoveAll(t =>
                            t.Name == transaction.Tool);
                        newWorkspaceDefinition.Templates.Add(new TerraformTemplate(transaction.Tool,
                            TerraformStatus.DeleteRequested, requestedDate));
                        break;
                    case ToolboxTransactionType.Update:
                        ApplyConfigurations(newWorkspaceDefinition, transaction);
                        break;
                }
            }

            return newWorkspaceDefinition;
        }

        /// <summary>
        /// Swaps the workspace's definition tool-related configuration with the new configuration provided
        /// </summary>
        /// <param name="workspaceDefinition">Workspace definition to apply this switch to</param>
        /// <param name="transaction">The transaction containing the new configuration information</param>
        private static void ApplyConfigurations(WorkspaceDefinition workspaceDefinition, ToolboxTransaction transaction)
        {
            (transaction.UpdatedData as IWorkspaceToolConfiguration)?.WriteToWorkspaceDefinition(workspaceDefinition);
        }
    }

    /// <summary>
    /// Various extensions to facilitate the use of ToolboxTransactions
    /// </summary>
    public static class ToolboxTransactionExtensions
    {
        public static ToolboxTransaction AddTool(this List<ToolboxTransaction> transactions, string tool,
            dynamic? updatedData)
        {
            var transaction = new ToolboxTransaction
            {
                Tool = tool,
                Type = ToolboxTransactionType.Add,
                UpdatedData = updatedData
            };
            transactions.Add(transaction);
            return transaction;
        }

        public static ToolboxTransaction RemoveTool(this List<ToolboxTransaction> transactions, string tool)
        {
            var transaction = new ToolboxTransaction
            {
                Tool = tool,
                Type = ToolboxTransactionType.Remove
            };
            transactions.Add(transaction);
            return transaction;
        }

        public static ToolboxTransaction UpdateTool(this List<ToolboxTransaction> transactions, string tool,
            IWorkspaceToolConfiguration? originalData, IWorkspaceToolConfiguration? updatedData)
        {
            var transaction = new ToolboxTransaction
            {
                Tool = tool,
                Type = ToolboxTransactionType.Update,
                OriginalData = originalData,
                UpdatedData = updatedData
            };
            transactions.Add(transaction);
            return transaction;
        }

        public static void Revert(this List<ToolboxTransaction> transactions, ToolboxTransaction transaction)
        {
            transactions.Remove(transaction);
        }

        public static bool ContainsTool(this IEnumerable<ToolboxTransaction> transactions, string toolName) => transactions.Select(t => t.Tool).Contains(toolName);
        public static bool DoesNotContainTool(this IEnumerable<ToolboxTransaction> transactions, string toolName) => !ContainsTool(transactions, toolName);

        /// <summary>
        /// This method will diff the original and updated data of a transaction. This is very useful
        /// to display the changes that were made to a workspace definition through either an Add or an Update.
        /// This uses reflection to avoid having to write a diff method for each type of configuration.
        /// </summary>
        /// <param name="transaction">The transaction containing the config data</param>
        /// <returns>
        /// Dictionary of string with property names as keys and tuples of (original value, updated value) as values.
        /// This dictionary will contain only the properties that have changed.
        /// </returns>
        /// <exception cref="InvalidOperationException">Throws an exception if UpdatedData is null or if OriginalData and UpdatedData are provided but not of the same type</exception>
        public static Dictionary<string, (object? Original, object Updated)> Diff(this ToolboxTransaction transaction)
        {
            if (transaction.OriginalData is null && transaction.UpdatedData is null)
            {
                return new Dictionary<string, (object? Original, object Updated)>();
            }

            if (transaction.UpdatedData == null)
            {
                throw new InvalidOperationException("UpdatedData must not be null");
            }

            if (transaction.OriginalData is not null &&
                transaction.OriginalData!.GetType() != transaction.UpdatedData!.GetType())
            {
                throw new InvalidOperationException("OriginalData and UpdatedData must be of same type");
            }

            // Diff the data
            PropertyInfo[] updatedProperties = transaction.UpdatedData.GetType()
                .GetProperties();

            if (transaction.OriginalData is not null)
            {
                PropertyInfo[] originalProperties = transaction.OriginalData.GetType()
                    .GetProperties();
                var differences = originalProperties
                    .Where(prop => updatedProperties.All(p => p.Name != prop.Name) ||
                                   !Equals(prop.GetValue(transaction.OriginalData),
                                       prop.GetValue(transaction.UpdatedData)))
                    .ToDictionary(prop => prop.Name,
                        prop => (Original: prop.GetValue(transaction.OriginalData),
                            Updated: updatedProperties.First(p => p.Name == prop.Name)
                                .GetValue(transaction.UpdatedData)));
                return differences;
            }
            else
            {
                var differences = updatedProperties
                    .Where(prop => prop.GetValue(transaction.UpdatedData) != null)
                    .ToDictionary(prop => prop.Name,
                        prop => (Original: (object?)null,
                            Updated: prop.GetValue(transaction.UpdatedData)));
                return differences;
            }
        }
    }
}