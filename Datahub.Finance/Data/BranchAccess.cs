
using Datahub.Core.Data;
using MudBlazor.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Portal.Data.Finance
{
    public class BranchAccess
    {
        [Key]
        public int BranchAccess_ID { get; set; }
        [MaxLength(20)]
        public string BranchFundCenter { get; set; }
        [MaxLength(100)]
        public string User { get; set; }
        public bool IsInactive { get; set; }
        

    }
}
