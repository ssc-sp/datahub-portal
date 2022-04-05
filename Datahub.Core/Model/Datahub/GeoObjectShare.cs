using Elemental.Components;
using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.EFCore
{
    public class GeoObjectShare
    {
        [AeFormIgnore]
        [Key]
        public string GeoObjectShare_ID { get; set; }
        [Required]
        public string Json_TXT { get; set; }
        public int? ApprovalForm_ID { get; set; }
        public bool ApprovalFormCompleted { get; set; }
        public bool ShareApproved { get; set; }
    }
}
