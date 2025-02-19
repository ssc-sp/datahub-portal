using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using Datahub.Application.Services.Toolbox;
using Datahub.Shared;
using Datahub.Shared.Entities;

namespace Datahub.Infrastructure.Services.Toolbox
{
    public class ToolboxService : IToolboxService
    {
        public List<ToolboxTransaction> BeginTransaction()
        {
            return new List<ToolboxTransaction>();
        }

        public WorkspaceDefinition ApplyTransaction(WorkspaceDefinition workspaceDefinition,
            List<ToolboxTransaction> transactions)
        {
            WorkspaceDefinition newWorkspaceDefinition;
            var wdString = JsonSerializer.Serialize(workspaceDefinition);
            newWorkspaceDefinition = JsonSerializer.Deserialize<WorkspaceDefinition>(wdString)!;

            foreach (var transaction in transactions)
            {
                switch (transaction.Type)
                {
                    case ToolboxTransactionType.Add:
                        newWorkspaceDefinition.Templates.Add(new TerraformTemplate(transaction.Tool,
                            TerraformStatus.CreateRequested));
                        ApplyConfigurations(newWorkspaceDefinition, transaction);
                        break;
                    case ToolboxTransactionType.Remove:
                        newWorkspaceDefinition.Templates.RemoveAll(t =>
                            t.Name == transaction.Tool);
                        newWorkspaceDefinition.Templates.Add(new TerraformTemplate(transaction.Tool,
                            TerraformStatus.DeleteRequested));
                        break;
                    case ToolboxTransactionType.Update:
                        ApplyConfigurations(newWorkspaceDefinition, transaction);
                        break;
                }
            }

            return newWorkspaceDefinition;
        }

        private void ApplyConfigurations(WorkspaceDefinition workspaceDefinition, ToolboxTransaction transaction)
        {
            switch (transaction.Tool)
            {
                case TerraformTemplate.AzurePostgres:
                    workspaceDefinition.AppData.PostgresConfiguration = transaction.UpdatedData;
                    break;
            }
        }
    }

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
            dynamic? originalData, dynamic? updatedData)
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
                    .ToDictionary(prop => prop.Name,
                        prop => (Original: (object?)null,
                            Updated: prop.GetValue(transaction.UpdatedData)));
                return differences;
            }
        }
    }
}