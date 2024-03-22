using System.ComponentModel.DataAnnotations;
using Elemental.Components;

namespace Datahub.Core.Model.Projects;

public class Datahub_ProjectApiUser
{
    [AeFormIgnore]
    [Key]
    public Guid ProjectApiUser_ID { get; set; }

    [Required]
    [StringLength(32)]
    [AeLabel("Name")]
    public string Client_Name_TXT { get; set; }

    [Required]
    [StringLength(10)]
    [AeLabel("Project")]
    public string Project_Acronym_CD { get; set; }

    [Required]
    [StringLength(128)]
    [AeLabel("Email")]
    public string Email_Contact_TXT { get; set; }

    [AeLabel("Expiration")]
    public DateTime? Expiration_DT { get; set; }

    public bool Enabled { get; set; }
}