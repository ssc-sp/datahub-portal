using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Datahub.Finance.Data;

public class FiscalYear
{

    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Required]
    public int YearId { get; set; }

    [MaxLength(20)]
    public string Year { get; set; }

    public List<FundCenter> FundCenters { get; set; }

}