using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NRCan.Datahub.Data.Projects
{
    public class WebForm
    {
        [Key] public int WebForm_ID { get; set; }
        [MaxLength(100)] [Required] public string Title_DESC { get; set; }
        [MaxLength(100)] public string Project_Name { get; set; }

        public List<WebForm_Field> Fields { get; set; }
    }

    public class WebForm_Field
    {
        [Key]
        public int FieldID { get; set; }

        /** Section:  **/
        [MaxLength(100)] public string? Section_DESC { get; set; }
        /** Section:  **/
        [Required] [MaxLength(100)] public string Field_DESC { get; set; }
        /** Section:  **/
        public string? Description { get; set; }
        /** Section:  **/
        [Required] [MaxLength(100)] public string Class { get; set; }
        /** Section:  **/
        [Required] [MaxLength(100)] public string Type { get; set; }
        /** Section:  **/
        [Required] [MaxLength(100)] public string Max_Length_NUM { get; set; }
        /** Section:  **/
        public string? Notes { get; set; }
        /** Section:  **/
        [Required] [MaxLength(100)] public bool Mandatory_FLAG { get; set; }
        /** Section:  **/
        [Required] public DateTime Date_Updated_DT { get; set; }

        public WebForm WebForm { get; set; }

    }

    public class WebForm_DBCodes
    {

        [Key][Required] [MaxLength(10)] public string DBCode { get; set; }

        [Required] [MaxLength(100)] public string ClassWord_DESC { get; set; }

        [Required] public string ClassWord_DEF { get; set; }
    }
}