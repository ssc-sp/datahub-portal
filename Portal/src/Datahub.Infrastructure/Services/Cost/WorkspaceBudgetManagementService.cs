using System.Runtime.CompilerServices;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Datahub.Application.Services.Budget;
using Azure.ResourceManager.Consumption;
using Azure.ResourceManager.CostManagement;
using Datahub.Application.Services.Azure;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Datahub.Infrastructure.UnitTests")]

namespace Datahub.Infrastructure.Services.Cost
{
    public class WorkspaceBudgetManagementService : IWorkspaceBudgetManagementService
    {
        private ArmClient _armClient;
        private readonly ILogger<WorkspaceBudgetManagementService> _logger;

        public WorkspaceBudgetManagementService(ILogger<WorkspaceBudgetManagementService> logger, IAzureResourceManagerClientProvider armClientProvider)
        {
            _armClient = armClientProvider.GetClient();
            _logger = logger;
        }

        public async Task<decimal> GetBudgetAmountAsync(string budgetId)
        {
            var budget = await GetBudgetAzureResource(budgetId);
            if (!budget.HasData)
            {
                _logger.LogError($"Could not parse data from budget with id {budgetId}");
                throw new Exception($"Could not parse data from budget with id {budgetId}");
            }

            return budget.Data.Amount!.Value;
        }

        public async Task SetBudgetAmountAsync(string budgetId, decimal amount)
        {
            var budget = await GetBudgetAzureResource(budgetId);
            if (!budget.HasData)
            {
                _logger.LogError($"Could not parse data from budget with id {budgetId}");
                throw new Exception($"Could not parse data from budget with id {budgetId}");
            }
            
            budget.Data.Amount = amount;
            await budget.UpdateAsync(WaitUntil.Completed, budget.Data);
        }
        
        public async Task<decimal> GetBudgetSpentAsync(string budgetId)
        {
            var budget = await GetBudgetAzureResource(budgetId);
            if (!budget.HasData)
            {
                _logger.LogError($"Could not parse data from budget with id {budgetId}");
                throw new Exception($"Could not parse data from budget with id {budgetId}");
            }
            return budget.Data.CurrentSpend.Amount!.Value;
        }

        private async Task<ConsumptionBudgetResource> GetBudgetAzureResource(string budgetId)
        {
            var budgetIdentifier = new ResourceIdentifier(budgetId);
            var budget = _armClient.GetConsumptionBudgetResource(budgetIdentifier);

            if (budget == null)
            {
                _logger.LogError($"Budget with id {budgetId} not found");
                throw new Exception($"Budget with id {budgetId} not found");
            }
            
            var response = await budget.GetAsync();

            if (response.Value == null)
            {
                _logger.LogError($"Budget with id {budgetId} not found");
                throw new Exception($"Budget with id {budgetId} not found");
            }

            return response.Value;
        }
    }
}