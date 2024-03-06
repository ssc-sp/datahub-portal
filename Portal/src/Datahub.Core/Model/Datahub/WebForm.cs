using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datahub.Core.Model.Projects;
using Elemental.Components;

namespace Datahub.Core.Model.Datahub;

public class WebForm
{
    [AeFormIgnore]
    [Key]
    public int WebFormID { get; set; }

    [StringLength(100)]
    [Required]
    [AeLabel("Title")]
    public string TitleDESC { get; set; }

    [AeLabel("Description")]
    public string DescriptionDESC { get; set; }

    // [MaxLength(100)]
    // public string Project_Name { get; set; }

    public List<WebFormField> Fields { get; set; }

    public DatahubProject Project { get; set; }

    [ForeignKey("Project")]
    [AeFormIgnore]
    public int ProjectID { get; set; }
}

public class WebFormDBCodes
{
    [Key]
    [Required]
    [MaxLength(10)]
    public string DBCode { get; set; }

    [Required]
    [MaxLength(100)]
    public string ClassWordDESC { get; set; }

    [Required]
    public string ClassWordDEF { get; set; }
}