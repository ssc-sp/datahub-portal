﻿using System.ComponentModel.DataAnnotations;

namespace Datahub.Portal.Data.PIP
{
    public class PIP_TombstoneRisks
    {
        [Key]
        public int TombstoneRisk_ID { get; set; }
        public PIP_Tombstone Pip_Tombstone { get; set; }
        public PIP_Risks Pip_Risk { get; set; }
    }
}
