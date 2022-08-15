using Elemental.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Portal.Data.Finance
{
    public class FundCenter
    {
        [Key]
        [AeFormIgnore]
        public int FundCenter_ID { get; set; }

        [Required]
        public int FiscalYear { get; set; }

        [AeLabel(isDropDown: true)]
        [Required]
        public string Sector { get; set; }

        [AeLabel(isDropDown: true)]
        [Required]
        public string Branch { get; set; }

        [AeLabel(isDropDown: true)]
        [Required]
        public string Division { get; set; }

        public double? AttritionRate { get; set; }

        [MaxLength(4000)]
        public string Description { get; set; }

        [AeFormIgnore]
        public string Last_Updated_UserId { get; set; }

        [AeFormIgnore]
        public DateTime Last_Updated_DT { get; set; }

        [AeFormIgnore]
        public string Created_UserId { get; set; }

        [AeFormIgnore]
        public DateTime Created_DT { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}
