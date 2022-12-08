using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Datahub.Portal.Data.Finance
{
    public class FiscalYear
    {
        
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public int YearId { get; set; }

        [MaxLength(20)]
        public string Year { get; set; }

        public List<FundCenter> FundCenters { get; set; }
        
    }
}
