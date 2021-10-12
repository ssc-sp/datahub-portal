using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.ProjectForms.Data.PIP
{
    public class PIP_TombstoneRisks
    {
        [Key]
        public int TombstoneRisk_ID { get; set; }
        public PIP_Tombstone Pip_Tombstone { get; set; }
        public PIP_Risks Pip_Risk { get; set; }
    }
}
