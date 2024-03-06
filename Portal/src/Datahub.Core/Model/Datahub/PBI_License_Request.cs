using System.ComponentModel.DataAnnotations;
using Datahub.Core.Model.Projects;
using Elemental.Components;

namespace Datahub.Core.Model.Datahub;

public enum DataSourceProtection
{
    Unclassified, ProtectedA, ProtectedB, Unknown
}

public class PBILicenseRequest
{
    [AeFormIgnore]
    [Key]
    public int RequestID { get; set; }

    [AeLabel("Is a Premium License required? ***(Refer to Appendix II for more details on licence types)")]
    [Required]
    public bool PremiumLicenseFlag { get; set; }

    [StringLength(200)]
    [Required]
    [AeLabel("Contact Email")]
    public string ContactEmail { get; set; }

    [StringLength(200)]
    [Required]
    [AeLabel("Contact Name")]
    public string ContactName { get; set; }

    public DatahubProject Project { get; set; }

    [AeFormIgnore]
    public int ProjectID { get; set; }

    public bool DesktopUsageFlag { get; set; }

    public List<PBIUserLicenseRequest> UserLicenseRequests { get; set; }

    [Required]
    public string UserID { get; set; }

    [AeFormIgnore]
    [Timestamp]
    public byte[] Timestamp { get; set; }
}