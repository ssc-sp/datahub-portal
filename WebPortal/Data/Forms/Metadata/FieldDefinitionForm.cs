using Elemental.Components;
using System.ComponentModel.DataAnnotations;

namespace Datahub.Portal.Data.Forms.Metadata
{
    public class FieldDefinitionForm
    {
        [Required]
        [StringLength(256)]
        [AeFormCategory("Name")]
        public string Field_Name_TXT { get; set; }

        [Required]
        [StringLength(256)]
        [AeFormCategory("Name")]
        public string Name_English_TXT { get; set; }

        [Required]
        [StringLength(256)]
        [AeFormCategory("Name")]
        public string Name_French_TXT { get; set; }

        [StringLength(256)]
        [AeFormCategory("Description")]
        public string English_DESC { get; set; }

        [StringLength(256)]
        [AeFormCategory("Description")]
        public string French_DESC { get; set; }

        [AeFormCategory("Other")]
        public bool Required_FLAG { get; set; }

        [AeFormCategory("Other")]
        public bool MultiSelect_FLAG { get; set; }

        [StringLength(256)]
        [AeFormCategory("Other")]
        public string Default_Value_TXT { get; set; }

        [AeFormCategory("Other")]
        public int Sort_Order_NUM { get; set; }

        [AeFormCategory("Other")]
        [AeLabel(placeholder: "Enter choice list separated by | characters. (new definitions)")]
        public string Choices_TXT { get; set; }
    }
}
