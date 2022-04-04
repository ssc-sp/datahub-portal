using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Datahub.Portal.Data.PIP
{
    public class PIP_FiscalYears
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public int YearId { get; set; }

        [MaxLength(20)]
        public string FiscalYear { get; set; }
    }
}
