
using MudBlazor.Forms;
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

        [AeFormCategory("Sector Information")]
        [Required]
        public FiscalYear FiscalYear { get; set; }

        [AeFormCategory("Sector Information")]
        [MudForm(IsDropDown = true)]
        [Required]
        public HierarchyLevel Sector { get; set; }

        [AeFormCategory("Sector Information")]
        [MudForm(IsDropDown = true)]
        [Required]
        public HierarchyLevel Branch { get; set; }

        [AeFormCategory("Sector Information")]
        [MudForm(IsDropDown = true)]
        [Required]
        public HierarchyLevel Division { get; set; }

        [AeFormCategory("Sector Information")]
        public double? AttritionRate { get; set; }

        [AeFormCategory("Sector Information")]
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
