using System.Reflection;
using Datahub.Application.Services.Toolbox;
using Datahub.Shared.Entities;

namespace Datahub.Infrastructure.Services.Toolbox
{
    public class ToolboxService : IToolboxService
    {
        public List<ToolboxTransaction> BeginTransaction()
        {
            return new List<ToolboxTransaction>();
        }
    }

    public static class ToolboxTransactionExtensions
    {
        public static ToolboxTransaction AddTool(this List<ToolboxTransaction> transactions, string tool)
        {
            var transaction = new ToolboxTransaction
            {
                Tool = tool,
                Type = ToolboxTransactionType.Add
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

        public static ToolboxTransaction UpdateTool(this List<ToolboxTransaction> transactions, string tool)
        {
            var transaction = new ToolboxTransaction
            {
                Tool = tool,
                Type = ToolboxTransactionType.Update
            };
            transactions.Add(transaction);
            return transaction;
        }

        public static void Revert(this List<ToolboxTransaction> transactions, ToolboxTransaction transaction)
        {
            transactions.Remove(transaction);
        }

        public static Dictionary<string, (object Original, object? Updated)> Diff(this ToolboxTransaction transaction)
        {
            if (transaction.OriginalData == null || transaction.UpdatedData == null)
            {
                throw new InvalidOperationException("OriginalData and UpdatedData must be set before calling Diff");
            }

            if (transaction.OriginalData!.GetType() != transaction.UpdatedData!.GetType())
            {
                throw new InvalidOperationException("OriginalData and UpdatedData must be of same type");
            }

            // Diff the data
            PropertyInfo[] originalProperties = transaction.OriginalData.GetType()
                .GetProperties();

            PropertyInfo[] updatedProperties = transaction.UpdatedData.GetType()
                .GetProperties();

            var differences = originalProperties
                .Where(prop => updatedProperties.All(p => p.Name != prop.Name) ||
                               !Equals(prop.GetValue(transaction.OriginalData), prop.GetValue(transaction.UpdatedData)))
                .ToDictionary(prop => prop.Name,
                    prop => (Original: prop.GetValue(transaction.OriginalData),
                        Updated: updatedProperties.First(p => p.Name == prop.Name).GetValue(transaction.UpdatedData)));


            return differences;
        }
    }
}