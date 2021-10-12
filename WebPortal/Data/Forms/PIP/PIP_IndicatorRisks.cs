using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.ProjectForms.Data.PIP
{
    public class PIP_IndicatorRisks
    {
        [Key]
        public int IndicatorRisk_ID { get; set; }
        public PIP_IndicatorAndResults Pip_Indicator { get; set; }
        public PIP_Risks Pip_Risk { get; set; }
    }
}
