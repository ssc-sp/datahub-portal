using System;
using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.EFCore
{
    public class Datahub_Project_Costs
    {
        [Key]
        public int ProjectCosts_ID { get; set; }

        public int Project_ID { get; set; }

        [StringLength(10)]
        public string Project_Acronym_CD { get; set; }

        public DateTime Usage_DT { get; set; }

        public double Cost_AMT { get; set; }

        public DateTime Updated_DT { get; set; }
    }
}
