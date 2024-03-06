using System.ComponentModel.DataAnnotations;
using Elemental.Components;

namespace Datahub.Core.Model.Datahub;

public class SpatialObjectShare
{
    [AeFormIgnore]
    [Key]
    [StringLength(40)]
    public string GeoObjectShareID { get; set; }

    [Required]
    public string JsonTXT { get; set; }

    [Required]
    [StringLength(128)]
    public string EmailContactTXT { get; set; }

    public int ApprovalFormID { get; set; }
    public SpatialObjectShareStatus ShareStatus { get; set; }
    public string ApprovalDocumentURL { get; set; }
    public string PublicationID { get; set; }
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