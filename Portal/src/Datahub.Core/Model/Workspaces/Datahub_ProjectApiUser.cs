using System.ComponentModel.DataAnnotations;
using MudBlazor.Forms;

namespace Datahub.Core.Model.Projects;

public class Datahub_ProjectApiUser
{
    [AeFormIgnore]
    [Key]
    public Guid ProjectApiUser_ID { get; set; }

    [Required]
    [StringLength(32)]
    [MudForm("Name")]
    public required string Client_Name_TXT { get; set; }

    [Required]
    [StringLength(10)]
    [MudForm("Project")]
    public required string Project_Acronym_CD { get; set; }

    [Required]
    [StringLength(128)]
    [MudForm("Email")]
    public required string Email_Contact_TXT { get; set; }

    [MudForm("Expiration")]
    public DateTime? Expiration_DT { get; set; }

    public bool Enabled { get; set; }
}