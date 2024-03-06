using System.ComponentModel.DataAnnotations;
using Elemental.Components;

namespace Datahub.Core.Model.Projects;

public class DatahubProjectApiUser
{
    [AeFormIgnore]
    [Key]
    public Guid ProjectApiUserID { get; set; }

    [Required]
    [StringLength(32)]
    [AeLabel("Name")]
    public string ClientNameTXT { get; set; }

    [Required]
    [StringLength(10)]
    [AeLabel("Project")]
    public string ProjectAcronymCD { get; set; }

    [Required]
    [StringLength(128)]
    [AeLabel("Email")]
    public string EmailContactTXT { get; set; }

    [AeLabel("Expiration")]
    public DateTime? ExpirationDT { get; set; }

    public bool Enabled { get; set; }
}