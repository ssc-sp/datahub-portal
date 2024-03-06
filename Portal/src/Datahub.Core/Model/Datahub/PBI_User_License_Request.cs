using System.ComponentModel.DataAnnotations;
using Elemental.Components;

namespace Datahub.Core.Model.Datahub;

public enum PBIUserLicenseType
{
    FreeUser, ProUser
}

public class PBIUserLicenseRequest
{
    [AeFormIgnore]
    public int ID { get; set; }

    [StringLength(200)]
    [Required]
    public string UserEmail { get; set; }

    [Required]
    [StringLength(10)]
    public string LicenseType { get; set; }

    public int RequestID { get; set; }

    public PBILicenseRequest LicenseRequest { get; set; }
}