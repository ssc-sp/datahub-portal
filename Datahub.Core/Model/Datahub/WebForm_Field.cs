using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.RegularExpressions;
using Elemental.Components;

namespace Datahub.Core.EFCore
{
    public class WebForm_Field
    {
        [Key]
        [AeFormIgnore]
        public int FieldID { get; set; }

        [StringLength(100)]
        [AeLabel("Section")]
        public string Section_DESC { get; set; }

        [Required]
        [StringLength(100)]
        [AeLabel("Field")]
        public string Field_DESC { get; set; }

        [AeLabel(label: "Description", placeholder: "Type a description to be used as a field placeholder.")]
        public string Description_DESC { get; set; }

        [AeLabel(label: "Choices", placeholder: "Type field choices separated by a pipe | character.")]
        public string Choices_TXT { get; set; }

        [Required]
        [StringLength(8)]
        [AeLabel(label: "Extension", isDropDown:true, validValues: new [] 
            {
                "NONE", "AMT", "AMTL", "AMTR", "CD", "CNT", "DT", "DESC", "DUR", "URL", "EMAIL", "NT", "FCTR", "ID", "FLAG", 
                "MULT", "NAME", "NUM", "PCT", "QTY", "RT", "RTO", "SID", "TXT", "IND", "TIME", "TS", "VAL"
            })]
        public string Extension_CD { get; set; } = "NONE";

        [AeFormIgnore]
        public string ExtensionLabel => ExtensionTypeReference.ClassWords[Extension_CD];

        [Required]
        [StringLength(8)]
        [AeLabel(label: "Field Type", isDropDown:true, validValues: new [] 
        {
            "Text", "Integer", "Decimal", "Boolean", "Dropdown", "Date", "Time", "Money"
        })]
        public string Type_CD { get; set; } = "Text";

        [AeLabel("Max Length")]
        public int? Max_Length_NUM { get; set; }

        [AeLabel("Notes")]
        public string Notes_TXT { get; set; }

        [Required]
        [AeLabel("Mandatory")]
        public bool Mandatory_FLAG { get; set; }

        [Required]
        [AeFormIgnore]
        public DateTime Date_Updated_DT { get; set; }

        public WebForm WebForm { get; set; }

        [ForeignKey("WebForm")]
        [AeFormIgnore]
        public int WebForm_ID { get; set; }

        public WebForm_Field Clone()
        {
            return new WebForm_Field()
            {
                Section_DESC = this.Section_DESC,
                Field_DESC = this.Field_DESC,
                Description_DESC = this.Description_DESC,
                Extension_CD = this.Extension_CD,
                Max_Length_NUM = this.Max_Length_NUM,
                Type_CD = this.Type_CD,
                Notes_TXT = this.Notes_TXT,
                Mandatory_FLAG = this.Mandatory_FLAG,
                Choices_TXT = this.Choices_TXT
            };
        }

        public void TakeValuesFrom(WebForm_Field other)
        {
            this.Section_DESC = other.Section_DESC;
            this.Field_DESC = other.Field_DESC;
            this.Description_DESC = other.Description_DESC;
            this.Extension_CD = other.Extension_CD;
            this.Max_Length_NUM = other.Max_Length_NUM;
            this.Type_CD = other.Type_CD;
            this.Notes_TXT = other.Notes_TXT;
            this.Mandatory_FLAG = other.Mandatory_FLAG;
            this.Choices_TXT = other.Choices_TXT;
        }
    }
}