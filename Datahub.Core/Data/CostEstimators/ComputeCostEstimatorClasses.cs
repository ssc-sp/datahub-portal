using System;
using System.Collections.Generic;

namespace Datahub.Core.Data.CostEstimator
{
    public enum ComputeCostEstimateWorkloadType
    {
        VeryLight,
        Light,
        Medium,
        Heavy
    }

    public class ComputeCostEstimatorResult
    {
        public EstimatorResultLineDecimal VmHours { get; set; }
        public EstimatorResultLineDecimal Dbu { get; set; }

        private decimal Cost(EstimatorResultLineDecimal l) => l?.Cost ?? 0.0000M;
        public decimal TotalCost => Cost(VmHours) + Cost(Dbu);
    }
    
    public enum ComputeCostMachineType
    {
        DS3,
        DS4,
        DS5
    }

    public class ComputeCostMachineSpec
    {
        public int Cores { get; set; }
        public int RamGB { get; set; }
        public decimal DbuFactor { get; set; }
        public decimal VmCost { get; set; }
    }

    public class ComputeCostEstimatorPrices
    {
        public DateTime LastUpdatedUtc { get; set; }
        public decimal DbuPrice { get; set; }
        public decimal Ds3VmPrice { get; set; }
        public decimal Ds4VmPrice { get; set; }
        public decimal Ds5VmPrice { get; set; }
    }
}
