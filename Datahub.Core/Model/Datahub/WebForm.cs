using Elemental.Components;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Datahub.Shared.EFCore
{
    public class WebForm
    {
        [AeFormIgnore]
        [Key] 
        public int WebForm_ID { get; set; }

        [StringLength(100)]
        [Required]
        [AeLabel("Title")]
        public string Title_DESC { get; set; }

        [AeLabel("Description")]
        public string Description_DESC { get; set; }

        // [MaxLength(100)]
        // public string Project_Name { get; set; }

        public List<WebForm_Field> Fields { get; set; }

        public Datahub_Project Project { get; set; }

        [ForeignKey("Project")]
        [AeFormIgnore]
        public int Project_ID { get; set; }
    }

    

    public class WebForm_DBCodes
    {

        [Key]
        [Required]
        [MaxLength(10)]
        public string DBCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string ClassWord_DESC { get; set; }

        [Required]
        public string ClassWord_DEF { get; set; }
    }
}