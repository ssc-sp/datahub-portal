using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.RegularExpressions;
using Elemental.Components;

namespace NRCan.Datahub.Data.Projects
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

        [AeLabel("Description")]
        public string Description_DESC { get; set; }

        [Required]
        [StringLength(8)]
        [AeLabel(label:"Extension", isDropDown:true, validValues: new [] 
            {
                "NONE", "AMT", "AMTL", "AMTR", "CD", "CNT", "DT", "DESC", "DUR", "URL", "EMAIL", "NT", "FCTR", "ID", "FLAG", 
                "MULT", "NAME", "NUM", "PCT", "QTY", "RT", "RTO", "SID", "TXT", "IND", "TIME", "TS", "VAL"
            })]
        public string Extension_CD { get; set; } = "NONE";

        [AeFormIgnore]
        public string ExtensionLabel
        {
            get
            {
                return ExtensionTypeReference.ClassWords[Extension_CD];
            }
        }

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

        [AeFormIgnore]
        public string Formatted
        {
            get
            {
                if (Field_DESC == null) return string.Empty;

                var deDashed = Field_DESC.Replace("-","");
                return Regex.Replace(deDashed, "[^A-Za-z0-9_]+", "_", RegexOptions.Compiled);
            }
        }

        [AeFormIgnore]
        public string Code
        {
            get 
            {
                return Extension_CD;
            }
        }

        [AeFormIgnore]
        public string SQLName
        {
            get
            {
                return Extension_CD == "NONE" ?
                    Formatted :
                    Formatted + "_" + Code;
            }
        }

        [AeFormIgnore]
        public string EFType
        {
            get
            {
                return FormFieldTypeReference.EFTypes[Type_CD];
            }
        }

        [AeFormIgnore]
        public string EFCoreAnnotations
        {
            get
            {
                var sb = new StringBuilder();
                // EFCoreAnnotation1: field type
                if (FormFieldTypeReference.Annotations.ContainsKey(Type_CD))
                {
                    sb.AppendLine(FormFieldTypeReference.Annotations[Type_CD]);
                }

                // EFCoreAnnotation2: required
                if (Mandatory_FLAG)
                {
                    sb.AppendLine("[Required]");
                }

                // EFCoreAnnotation3: MaxLength
                if (Max_Length_NUM.HasValue)
                {
                    sb.AppendLine($"[MaxLength({Max_Length_NUM.Value})]");
                }

                return sb.ToString();
            }
        }

        [AeFormIgnore]
        public string JSON
        {
            get
            {
                return $"\"{SQLName}\": \"{System.Web.HttpUtility.JavaScriptStringEncode(Field_DESC)}\"";
            }
        }

        [AeFormIgnore]
        public string CSCode
        {
            get
            {
                // /** Section: Outcome Level **/ [Required][MaxLength(100)]public string Outcome_Level_DESC {get;set;}
                // =CONCATENATE("/** Section: ",[@Section], " **/ ",
                //          [@[EF Core Annotation1]],[@[EF Core Annotation2]],[@[EF Core Annotation3]],
                //          "public ",[@[EF Type]]," ",[@[SQL Name]]," {get;set;}")
                var sb = new StringBuilder();

                sb.AppendLine($"/** Section: {Section_DESC} **/");
                // EFCoreAnnotations already has a line break at the end
                sb.Append(EFCoreAnnotations);
                sb.AppendLine($"public {EFType} {SQLName} {{ get; set; }}");

                return sb.ToString();
            }
        }

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
                Mandatory_FLAG = this.Mandatory_FLAG
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
        }
    }
}