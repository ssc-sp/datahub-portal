using Datahub.Application.Services.Cost;
using Datahub.Core.Model.Projects;
using Datahub.Infrastructure.Services.Azure;
using System.Text.Json;

namespace Datahub.Portal.Pages.Workspace.Reports
{
    public static class BudgetCostReportUtils
    {
        public static List<DailyServiceCost> DeserializeCredits<T>(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<List<DailyServiceCost>>(json);
            }
            catch
            {
                var costs = JsonSerializer.Deserialize<List<T>>(json);
                return ConvertCosts(costs);
            }
        }

        public static List<DailyServiceCost> ConvertCosts<T>(List<T> costs)
        {
            if (typeof(T) == typeof(AzureDailyCost))
            {
                var typedCosts = costs as List<AzureDailyCost>;
                return typedCosts!.Select(c => new DailyServiceCost
                {
                    Amount = (decimal)c.Cost,
                    Date = c.Date
                }).ToList();
            }

            if (typeof(T) == typeof(AzureServiceCost))
            {
                var typedCosts = costs as List<AzureServiceCost>;
                return typedCosts!.Select(c => new DailyServiceCost
                {
                    Amount = (decimal)c.Cost,
                    Source = c.Name
                }).ToList();
            }

            if (typeof(T) == typeof(Datahub_Project_Costs))
            {
                var typedCosts = costs as List<Datahub_Project_Costs>;
                return typedCosts!.Select(c => new DailyServiceCost
                {
                    Amount = (decimal)c.CadCost,
                    Date = c.Date,
                    Source = c.ServiceName
                }).ToList();
            }

            throw new Exception("Invalid type");
        }

        public static decimal GetTotalNonBlankAmount(IEnumerable<DailyServiceCost> costs) => costs
            .Where(c => !string.IsNullOrEmpty(c.Source))
            .Sum(c => c.Amount);

        public static decimal GetTotalAmount(IEnumerable<DailyServiceCost> costs) => costs
            .Sum(c => c.Amount);

        public static string DisplayDollar(decimal amount) => 
            amount >= 0.01m ? amount.ToString("C2") :
            amount > 0 ? $"< {0.01m:C2}" :
            0.0m.ToString("C2");
    }
}
