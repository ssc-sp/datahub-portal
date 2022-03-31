using Elemental.Components;
using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.EFCore
{
    public class GeoObjectShare
    {
        [AeFormIgnore]
        [Key]
        [StringLength(40)]
        public string GeoObjectShare_ID { get; set; }
        [Required]
        public string Json_TXT { get; set; }
        public int? ApprovalForm_ID { get; set; }
        public ShareFGPStatus ShareStatus { get; set; }
    }

    public enum ShareFGPStatus
    {
        FillApprovalForm,
        SubmitApprovalForm,
        WaitingForApproval,
        PublishDataset,
        DatasetPublished
    }
}
