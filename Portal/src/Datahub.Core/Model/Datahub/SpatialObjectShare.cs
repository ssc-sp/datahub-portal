using System.ComponentModel.DataAnnotations;
using Elemental.Components;

namespace Datahub.Core.Model.Datahub;

public class SpatialObjectShare
{
    [AeFormIgnore]
    [Key]
    [StringLength(40)]
    public string GeoObjectShare_ID { get; set; }

    [Required]
    public string Json_TXT { get; set; }

    [Required]
    [StringLength(128)]
    public string Email_Contact_TXT { get; set; }

    public int ApprovalForm_ID { get; set; }
    public SpatialObjectShareStatus ShareStatus { get; set; }
    public string Approval_Document_URL { get; set; }
    public string Publication_ID { get; set; }
    public bool Deleted { get; set; }
}

public enum SpatialObjectShareStatus
{
    FillApprovalForm,
    SubmitApprovalForm,
    WaitingForApproval,
    PublishDataset,
    DatasetPublished
}